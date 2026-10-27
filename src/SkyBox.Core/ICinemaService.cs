namespace SkyBox.Core;

// A storage boundary for a future SQLite implementation. No web or console dependencies.
public interface ICinemaService
{
    DateTime LocalNow { get; }
    IReadOnlyList<Movie> Movies { get; }
    IReadOnlyList<Room> Rooms { get; }
    IReadOnlyList<Screening> GetScreenings(DateOnly? date = null, int? movieId = null, int? roomId = null);
    Screening? GetScreening(Guid id);
    IReadOnlyList<Seat> GetSeats(Guid screeningId);
    Booking Reserve(Guid screeningId, IEnumerable<string> seats);
    Booking? FindBooking(string? code);
    Booking Cancel(string code);
    Screening AddScreening(int movieId, int roomId, DateTime startsAt, decimal price, string format);
    CinemaReport GetReport(DateOnly? date = null);
}
