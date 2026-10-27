using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBox.Core;

namespace SkyBox.Web.Pages;

public sealed class OperationsModel(ICinemaService cinema) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? Date { get; set; }
    [BindProperty] public ScreeningInput Input { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public IReadOnlyList<Movie> Movies => cinema.Movies;
    public IReadOnlyList<Room> Rooms => cinema.Rooms;
    public IReadOnlyList<Screening> Screenings { get; private set; } = [];
    public CinemaReport Report { get; private set; } = null!;
    public DateTime Minimum => cinema.LocalNow;

    public void OnGet()
    {
        Input.StartsAt = cinema.LocalNow.Date.AddDays(3).AddHours(18);
        Load();
    }
    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            try
            {
                var screening = cinema.AddScreening(Input.MovieId, Input.RoomId, Input.StartsAt!.Value,
                    Input.Price!.Value, Input.Format);
                Notice = $"Función agregada: {screening.Movie.Title}, {screening.Room.Name}, {screening.StartsAt:dd/MM HH:mm}.";
                return RedirectToPage(new { date = screening.StartsAt.ToString("yyyy-MM-dd") });
            }
            catch (CinemaException exception) { ModelState.AddModelError("", exception.Message); }
        }
        Load();
        return Page();
    }
    private void Load()
    {
        Date ??= DateOnly.FromDateTime(cinema.LocalNow);
        Screenings = cinema.GetScreenings(Date);
        Report = cinema.GetReport(Date);
    }

    public sealed class ScreeningInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una película.")]
        public int MovieId { get; set; }
        [Range(1, 4, ErrorMessage = "Selecciona una sala.")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Indica fecha y hora.")]
        public DateTime? StartsAt { get; set; }
        [Required(ErrorMessage = "Indica el precio."), Range(1, 500, ErrorMessage = "El precio debe estar entre Bs 1 y Bs 500.")]
        [ModelBinder(BinderType = typeof(Binding.InvariantDecimalBinder))]
        public decimal? Price { get; set; } = 30;
        [Required] public string Format { get; set; } = "2D DOB";
    }
}
