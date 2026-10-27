using System.Globalization;
using SkyBox.Core;

var culture = CultureInfo.GetCultureInfo("es-BO");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ICinemaService, InMemoryCinemaService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
