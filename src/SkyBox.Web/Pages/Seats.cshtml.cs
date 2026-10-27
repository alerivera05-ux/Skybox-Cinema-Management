using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBox.Core;

namespace SkyBox.Web.Pages;

public sealed class SeatsModel(ICinemaService cinema) : PageModel
{
    public Screening Screening { get; private set; } = null!;
    public IReadOnlyList<Seat> Seats { get; private set; } = [];
    [BindProperty] public string[] SelectedSeats { get; set; } = [];
    public bool CanBook => Screening.StartsAt > cinema.LocalNow && Screening.AvailableSeats > 0;
    public IActionResult OnGet(Guid id) => Load(id) ? Page() : NotFound();
    public IActionResult OnPost(Guid id)
    {
        if (!Load(id)) return NotFound();
        if (!ModelState.IsValid) return Page();
        try
        {
            var booking = cinema.Reserve(id, SelectedSeats);
            return RedirectToPage("/Ticket", new { code = booking.Code });
        }
        catch (CinemaException exception)
        {
            ModelState.AddModelError("", exception.Message);
            Load(id);
            return Page();
        }
    }
    private bool Load(Guid id)
    {
        var screening = cinema.GetScreening(id);
        if (screening is null) return false;
        Screening = screening;
        Seats = cinema.GetSeats(id);
        return true;
    }
}
