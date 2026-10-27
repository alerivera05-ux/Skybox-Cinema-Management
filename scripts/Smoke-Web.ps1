param([int]$Port = 5081, [string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$web = Join-Path $repo 'src/SkyBox.Web'
$dll = Join-Path $web "bin/$Configuration/net10.0/SkyBox.Web.dll"
if (!(Test-Path $dll)) { throw 'Build the solution before running the HTTP tests.' }
$start = [System.Diagnostics.ProcessStartInfo]::new('dotnet')
$start.ArgumentList.Add($dll)
$start.ArgumentList.Add('--urls')
$start.ArgumentList.Add("http://127.0.0.1:$Port")
$start.WorkingDirectory = $web
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.Environment['ASPNETCORE_ENVIRONMENT'] = 'Production'
$process = [System.Diagnostics.Process]::Start($start)
$outputTask = $process.StandardOutput.ReadToEndAsync()
$errorTask = $process.StandardError.ReadToEndAsync()
$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$client = [System.Net.Http.HttpClient]::new($handler)
$client.BaseAddress = "http://127.0.0.1:$Port"
$client.Timeout = [TimeSpan]::FromSeconds(10)
$checks = 0
function Check([bool]$Condition, [string]$Name) {
    if (!$Condition) { throw "FAIL $Name" }
    $script:checks++
    Write-Output "PASS $Name"
}
function Get-Page([string]$Path) {
    $response = $client.GetAsync($Path).GetAwaiter().GetResult()
    return @{ Status = [int]$response.StatusCode; Html = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); Response = $response }
}
function Token([string]$Html) {
    $match = [regex]::Match($Html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    if (!$match.Success) { throw 'Antiforgery token missing' }
    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}
function Post-Form([string]$Path, [hashtable]$Fields) {
    $pairs = [System.Collections.Generic.List[System.Collections.Generic.KeyValuePair[string,string]]]::new()
    foreach ($key in $Fields.Keys) {
        foreach ($value in @($Fields[$key])) { $pairs.Add([System.Collections.Generic.KeyValuePair[string,string]]::new($key, [string]$value)) }
    }
    $content = [System.Net.Http.FormUrlEncodedContent]::new($pairs)
    $response = $client.PostAsync($Path, $content).GetAwaiter().GetResult()
    return @{ Status = [int]$response.StatusCode; Html = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); Response = $response }
}
try {
    $homePage = $null
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        try { $homePage = Get-Page '/'; break } catch { Start-Sleep -Milliseconds 250 }
    }
    Check ($null -ne $homePage -and $homePage.Status -eq 200) 'Home responds in Production'
    Check ($homePage.Html -match 'En cartelera' -and $homePage.Html -match 'Superman') 'Billboard renders the movie catalog'
    Check ($homePage.Response.Headers.Contains('Content-Security-Policy')) 'Content security policy present'
    foreach ($path in '/css/site.css', '/js/site.js', '/favicon.svg', '/mis-reservas', '/operacion') {
        $page = Get-Page $path
        Check ($page.Status -eq 200) "GET $path"
    }
    Check ((Get-Page '/funcion/00000000-0000-0000-0000-000000000000').Status -eq 404) 'Unknown screening returns 404'
    Check ((Get-Page '/reserva/unknown').Status -eq 404) 'Unknown booking returns 404'
    $tomorrow = [DateTimeOffset]::UtcNow.ToOffset([TimeSpan]::FromHours(-4)).Date.AddDays(1).ToString('yyyy-MM-dd')
    $billboard = Get-Page "/?date=$tomorrow&MovieId=1&RoomId=2"
    Check ($billboard.Html -notmatch '<h3>El Conjuro 4</h3>') 'Movie and room filters apply'
    $seatPath = [regex]::Match($billboard.Html, 'href="(/funcion/[a-f0-9-]+)"').Groups[1].Value
    Check (![string]::IsNullOrWhiteSpace($seatPath)) 'Future screening is bookable'
    $seatPage = Get-Page $seatPath
    Check (([regex]::Matches($seatPage.Html, 'name="SelectedSeats"')).Count -eq 220) 'Seat map has 220 seats'
    $csrf = Token $seatPage.Html
    $noToken = Post-Form $seatPath @{ SelectedSeats = 'A01' }
    Check ($noToken.Status -eq 400) 'Reservation without CSRF token is rejected'
    $invalid = Post-Form $seatPath @{ __RequestVerificationToken = $csrf; SelectedSeats = 'Z99' }
    Check ($invalid.Status -eq 200 -and $invalid.Html -match 'no existe') 'Invalid seat shows controlled validation'
    $reserved = Post-Form $seatPath @{ __RequestVerificationToken = $csrf; SelectedSeats = @('A01', 'A02') }
    Check ($reserved.Status -eq 302) 'Group booking redirects to ticket'
    $ticketPath = $reserved.Response.Headers.Location.ToString()
    $ticket = Get-Page $ticketPath
    Check ($ticket.Status -eq 200 -and $ticket.Html -match 'A01, A02' -and $ticket.Html -match '50[.,]00') 'Ticket shows seats and server-calculated total'
    $code = $ticketPath.Split('/')[-1]
    $conflict = Post-Form $seatPath @{ __RequestVerificationToken = $csrf; SelectedSeats = @('A02', 'A03') }
    Check ($conflict.Status -eq 200 -and $conflict.Html -match 'acaba de reservar') 'Stale selection is rejected'
    $map = Get-Page $seatPath
    Check ($map.Html -match 'value="A03"[^>]*aria-label="Asiento A03, disponible"') 'Failed group leaves free seat untouched'
    $lookup = Get-Page "/mis-reservas?code=$code"
    Check ($lookup.Html -match 'CONFIRMADA') 'Booking lookup succeeds'
    $cancel = Post-Form '/mis-reservas?handler=Cancel' @{ __RequestVerificationToken = (Token $lookup.Html); Code = $code }
    Check ($cancel.Status -eq 302) 'Cancellation redirects to cancelled ticket'
    Check ((Get-Page $ticketPath).Html -match 'CANCELADA') 'Cancelled state persists'
    $again = Post-Form $seatPath @{ __RequestVerificationToken = $csrf; SelectedSeats = 'A01' }
    Check ($again.Status -eq 302) 'Cancelled seat can be reserved again'
    $operations = Get-Page '/operacion'
    $date = [DateTimeOffset]::UtcNow.ToOffset([TimeSpan]::FromHours(-4)).Date.AddDays(4)
    $fields = @{ __RequestVerificationToken = (Token $operations.Html); 'Input.MovieId' = '1'; 'Input.RoomId' = '1'; 'Input.StartsAt' = $date.AddHours(18).ToString('yyyy-MM-ddTHH:mm'); 'Input.Price' = '30.50'; 'Input.Format' = '2D DOB' }
    $created = Post-Form '/operacion' $fields
    Check ($created.Status -eq 302) 'Operator can schedule with decimal price 30.50'
    $collision = Post-Form '/operacion' $fields
    Check ($collision.Status -eq 200 -and $collision.Html -match 'se cruza') 'Conflicting schedule is rejected in the web form'
    $createdBillboard = Get-Page ('/?date=' + $date.ToString('yyyy-MM-dd'))
    Check ($createdBillboard.Html -match '30[.,]5') 'New screening appears in billboard with exact price'
    Write-Output "$checks HTTP checks passed"
} finally {
    $client.Dispose()
    if (!$process.HasExited) { $process.Kill($true) }
    $process.WaitForExit()
    $logs = Join-Path $repo '.dotnet-local'
    New-Item -ItemType Directory -Force $logs | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $logs 'http-tests.log'), $outputTask.GetAwaiter().GetResult() + $errorTask.GetAwaiter().GetResult())
    $process.Dispose()
}
