using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBox.Core;

namespace SkyBox.Web.Pages;

public sealed class TicketModel(ICinemaService cinema) : PageModel
{
    public Booking Booking { get; private set; } = null!;
    public IActionResult OnGet(string? code)
    {
        var booking = cinema.FindBooking(code);
        if (booking is null) return NotFound();
        Booking = booking;
        return Page();
    }
}
