using System.Collections.Concurrent;
using SkyBox.Core;

// Dependency-free executable tests. Run with dotnet run --project tests/SkyBox.Core.Tests.
var tests = new (string Name, Action Run)[]
{
    ("Seed contains 4 rooms, 7 movies and three days sorted by time", () =>
    {
        var cinema = new InMemoryCinemaService(new TestClock());
        Check(cinema.Rooms.Count == 4 && cinema.Movies.Count == 7);
        var all = cinema.GetScreenings();
        Check(all.Count == 36 && all.All(s => s.AvailableSeats == 220));
        Check(all.SequenceEqual(all.OrderBy(s => s.StartsAt).ThenBy(s => s.Room.Id)));
        Check(cinema.GetScreenings(new DateOnly(2030, 1, 2)).Count == 12);
        Check(cinema.GetScreenings(movieId: 1, roomId: 2).Count == 6);
    }),
    ("Group reservation normalizes seats and calculates exact total", () =>
    {
        var (cinema, screening, _) = Setup();
        var booking = cinema.Reserve(screening.Id, [" a01 ", "B02"]);
        Check(booking.Total == 61m && booking.Seats.SequenceEqual(new[] { "A01", "B02" }));
        Check(cinema.GetScreening(screening.Id)!.AvailableSeats == 218);
        Check(cinema.FindBooking(booking.Code.ToLowerInvariant()) == booking);
    }),
    ("A conflicting group is rejected atomically", () =>
    {
        var (cinema, screening, _) = Setup();
        cinema.Reserve(screening.Id, ["A01"]);
        Reject(() => cinema.Reserve(screening.Id, ["A02", "A01"]));
        Check(!cinema.GetSeats(screening.Id).Single(s => s.Label == "A02").Occupied);
        Check(cinema.GetReport().ReservedSeats == 1);
    }),
    ("Invalid, duplicate, empty and oversized selections leave no bookings", () =>
    {
        var (cinema, screening, _) = Setup();
        foreach (var selected in new string[][] { [], ["A01", "a01"], ["A00"], ["L01"], ["A21"], ["<script>"], ["A01", "A02", "A03", "A04", "A05", "A06", "A07", "A08", "A09"] })
            Reject(() => cinema.Reserve(screening.Id, selected));
        Check(cinema.GetReport().ActiveBookings == 0 && cinema.GetReport().ReservedSeats == 0);
    }),
    ("Eight seats are allowed and all labels A01 through K20 exist", () =>
    {
        var (cinema, screening, _) = Setup();
        var seats = cinema.GetSeats(screening.Id);
        Check(seats.Count == 220 && seats[0].Label == "A01" && seats[^1].Label == "K20");
        cinema.Reserve(screening.Id, Enumerable.Range(1, 8).Select(n => $"K{n:00}"));
        Check(cinema.GetReport().ReservedSeats == 8);
    }),
    ("Concurrent attempts produce exactly one winner for a seat", () =>
    {
        var (cinema, screening, _) = Setup();
        var winners = new ConcurrentBag<Booking>();
        Parallel.For(0, 100, _ =>
        {
            try { winners.Add(cinema.Reserve(screening.Id, ["A01"])); }
            catch (CinemaException) { }
        });
        Check(winners.Count == 1 && cinema.GetReport().ReservedSeats == 1);
    }),
    ("Cancellation releases all seats and adjusts report value", () =>
    {
        var (cinema, screening, _) = Setup();
        var booking = cinema.Reserve(screening.Id, ["A01", "A02"]);
        Check(cinema.GetReport().ReservedValue == 61m);
        Check(cinema.Cancel(booking.Code).Cancelled);
        var report = cinema.GetReport();
        Check(report.ReservedSeats == 0 && report.ActiveBookings == 0 && report.ReservedValue == 0);
        Reject(() => cinema.Cancel(booking.Code));
        var next = cinema.Reserve(screening.Id, ["A01"]);
        Check(next.Code != booking.Code && cinema.FindBooking(booking.Code)!.Cancelled);
    }),
    ("Old cancellation cannot free seats held by another booking", () =>
    {
        var (cinema, screening, _) = Setup();
        var first = cinema.Reserve(screening.Id, ["A01"]);
        cinema.Cancel(first.Code);
        cinema.Reserve(screening.Id, ["A01"]);
        Reject(() => cinema.Cancel(first.Code));
        Check(cinema.GetSeats(screening.Id).Single(s => s.Label == "A01").Occupied);
    }),
    ("Started screenings cannot be booked or cancelled", () =>
    {
        var (cinema, screening, clock) = Setup();
        var booking = cinema.Reserve(screening.Id, ["A01"]);
        clock.UtcNow = new DateTimeOffset(2030, 1, 2, 22, 0, 0, TimeSpan.Zero);
        Reject(() => cinema.Reserve(screening.Id, ["A02"]));
        Reject(() => cinema.Cancel(booking.Code));
    }),
    ("Room collision and cleanup buffer are enforced", () =>
    {
        var (cinema, _, _) = Setup();
        Reject(() => cinema.AddScreening(2, 1, new(2030, 1, 2, 18, 0, 0), 30, "2D DOB"));
        Reject(() => cinema.AddScreening(2, 1, new(2030, 1, 2, 20, 23, 0), 30, "2D DOB"));
        Reject(() => cinema.AddScreening(2, 1, new(2030, 1, 2, 16, 0, 0), 30, "2D DOB"));
        cinema.AddScreening(2, 1, new(2030, 1, 2, 20, 24, 0), 30, "2D SUB");
        cinema.AddScreening(2, 2, new(2030, 1, 2, 18, 0, 0), 30, "2D DOB");
        Check(cinema.GetScreenings().Count == 3);
    }),
    ("Invalid movie, room, price, format and schedule are rejected", () =>
    {
        var (cinema, _, _) = Setup();
        var date = new DateTime(2030, 1, 3, 18, 0, 0);
        Reject(() => cinema.AddScreening(999, 2, date, 30, "2D DOB"));
        Reject(() => cinema.AddScreening(1, 999, date, 30, "2D DOB"));
        foreach (var price in new[] { 0m, -1m, 501m, 30.001m }) Reject(() => cinema.AddScreening(1, 2, date, price, "2D DOB"));
        Reject(() => cinema.AddScreening(1, 2, date, 30, "INVALID"));
        Reject(() => cinema.AddScreening(1, 2, date.AddYears(1), 30, "2D DOB"));
        Reject(() => cinema.AddScreening(1, 2, date.AddDays(-2), 30, "2D DOB"));
        Check(cinema.GetScreenings().Count == 1);
    }),
    ("Snapshots cannot mutate service state", () =>
    {
        var (cinema, screening, _) = Setup();
        var snapshot = cinema.GetScreening(screening.Id)!;
        var booking = cinema.Reserve(screening.Id, ["A01"]);
        Check(snapshot.ReservedSeats == 0 && cinema.GetScreening(screening.Id)!.ReservedSeats == 1);
        Check(booking.Seats is not string[]);
        Check(cinema.Movies is not Movie[] && cinema.Rooms is not Room[]);
    }),
    ("Unknown screening or code returns a controlled outcome", () =>
    {
        var (cinema, _, _) = Setup();
        Check(cinema.GetScreening(Guid.Empty) is null && cinema.FindBooking(null) is null);
        Reject(() => cinema.GetSeats(Guid.Empty));
        Reject(() => cinema.Reserve(Guid.Empty, ["A01"]));
        Reject(() => cinema.Cancel("missing"));
    }),
    ("Empty report and date isolation have correct totals", () =>
    {
        var (cinema, screening, _) = Setup();
        cinema.Reserve(screening.Id, ["A01", "A02"]);
        var report = cinema.GetReport(new DateOnly(2030, 1, 2));
        Check(report.Capacity == 220 && report.ReservedSeats == 2 && report.ActiveBookings == 1 && report.ReservedValue == 61);
        Check(report.Occupancy > 0 && report.Occupancy < 1);
        Check(cinema.GetReport(new DateOnly(2030, 1, 3)).Occupancy == 0);
    }),
    ("Cinema clock uses Bolivia time independently of the server timezone", () =>
    {
        var clock = new TestClock { UtcNow = new(2030, 1, 2, 2, 0, 0, TimeSpan.Zero) };
        var cinema = new InMemoryCinemaService(clock, seed: false);
        Check(cinema.LocalNow == new DateTime(2030, 1, 1, 22, 0, 0));
    })
};

var failed = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception exception) { failed++; Console.WriteLine($"FAIL {test.Name}: {exception.Message}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} passed");
return failed == 0 ? 0 : 1;

static void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
static void Reject(Action action)
{
    try { action(); } catch (CinemaException) { return; }
    throw new Exception("Expected CinemaException");
}
static (InMemoryCinemaService Cinema, Screening Screening, TestClock Clock) Setup()
{
    var clock = new TestClock();
    var cinema = new InMemoryCinemaService(clock, seed: false);
    var screening = cinema.AddScreening(1, 1, new(2030, 1, 2, 18, 0, 0), 30.50m, "2D DOB");
    return (cinema, screening, clock);
}

sealed class TestClock : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2030, 1, 2, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => UtcNow;
}
