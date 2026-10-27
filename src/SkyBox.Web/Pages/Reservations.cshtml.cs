using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBox.Core;

namespace SkyBox.Web.Pages;

public sealed class ReservationsModel(ICinemaService cinema) : PageModel
{
    [BindProperty(SupportsGet = true), StringLength(40)] public string? Code { get; set; }
    public Booking? Booking { get; private set; }
    public bool CanCancel => Booking is { Cancelled: false } && Booking.StartsAt > cinema.LocalNow;
    public void OnGet()
    {
        if (string.IsNullOrWhiteSpace(Code) || !ModelState.IsValid) return;
        Booking = cinema.FindBooking(Code);
        if (Booking is null) ModelState.AddModelError("", "No encontramos ese código. Revísalo o verifica si el servidor se reinició.");
    }
    public IActionResult OnPostCancel()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var booking = cinema.Cancel(Code ?? "");
            return RedirectToPage("/Ticket", new { code = booking.Code });
        }
        catch (CinemaException exception)
        {
            ModelState.AddModelError("", exception.Message);
            Booking = cinema.FindBooking(Code);
            return Page();
        }
    }
}
