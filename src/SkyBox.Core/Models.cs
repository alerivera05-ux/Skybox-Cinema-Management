namespace SkyBox.Core;

public sealed record Movie(int Id, string Title, string Genre, int DurationMinutes,
    string Rating, string Synopsis, string Artwork, string Tagline);
public sealed record Room(int Id, string Name, int Rows = 11, int Columns = 20)
{
    public int Capacity => Rows * Columns;
}
public sealed record Seat(string Label, bool Occupied);
public sealed record Screening(Guid Id, Movie Movie, Room Room, DateTime StartsAt,
    decimal Price, string Format, int ReservedSeats)
{
    public int AvailableSeats => Room.Capacity - ReservedSeats;
    public decimal Occupancy => 100m * ReservedSeats / Room.Capacity;
}
public sealed record Booking(string Code, Guid ScreeningId, string Movie, string Room,
    DateTime StartsAt, IReadOnlyList<string> Seats, decimal UnitPrice, DateTime CreatedAt,
    bool Cancelled = false)
{
    public decimal Total => UnitPrice * Seats.Count;
}
public sealed record CinemaReport(int Screenings, int Capacity, int ReservedSeats,
    int ActiveBookings, decimal ReservedValue)
{
    public decimal Occupancy => Capacity == 0 ? 0 : 100m * ReservedSeats / Capacity;
}

public sealed class CinemaException(string message) : Exception(message);
