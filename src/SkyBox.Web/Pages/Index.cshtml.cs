using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBox.Core;

namespace SkyBox.Web.Pages;

public sealed class IndexModel(ICinemaService cinema) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Date { get; set; }
    [BindProperty(SupportsGet = true)] public int? MovieId { get; set; }
    [BindProperty(SupportsGet = true)] public int? RoomId { get; set; }
    public DateOnly Today => DateOnly.FromDateTime(cinema.LocalNow);
    public DateTime Now => cinema.LocalNow;
    public IReadOnlyList<Movie> Movies => cinema.Movies;
    public IReadOnlyList<Room> Rooms => cinema.Rooms;
    public IReadOnlyList<Screening> Screenings { get; private set; } = [];
    public Movie Featured => Movies[0];
    public void OnGet()
    {
        Date ??= cinema.GetScreenings(Today).Any(s => s.StartsAt > Now) ? Today : Today.AddDays(1);
        Screenings = cinema.GetScreenings(Date, MovieId, RoomId);
    }
}
