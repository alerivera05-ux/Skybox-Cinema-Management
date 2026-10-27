using System.Security.Cryptography;

namespace SkyBox.Core;

public sealed class InMemoryCinemaService : ICinemaService
{
    private readonly object gate = new();
    private readonly TimeProvider clock;
    private readonly Dictionary<Guid, Screening> screenings = [];
    private readonly Dictionary<string, Booking> bookings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, HashSet<string>> occupied = [];

    // Cinema schedules use Bolivia time (UTC-04:00, no daylight-saving changes).
    public DateTime LocalNow => clock.GetUtcNow().ToOffset(TimeSpan.FromHours(-4)).DateTime;
    public IReadOnlyList<Movie> Movies { get; } = Array.AsReadOnly(new Movie[]
    {
        new(1, "Superman", "Acción · Ciencia ficción", 129, "+12", "Un héroe entre dos mundos descubre que su mayor fuerza es su humanidad.", "superman", "MIRA HACIA ARRIBA"),
        new(2, "La hora de la desaparición", "Suspenso · Misterio", 128, "+16", "Una desaparición colectiva transforma una pequeña comunidad. Cada respuesta abre una nueva pregunta.", "weapons", "TODOS DESAPARECIERON. ¿POR QUÉ?"),
        new(3, "Otro viernes de locos", "Comedia · Familiar", 111, "ATP", "Una familia, un inesperado intercambio y un viernes que nadie podrá olvidar.", "friday", "UN DÍA. MUCHAS VIDAS."),
        new(4, "El Conjuro 4", "Terror · Sobrenatural", 135, "+16", "Un nuevo caso lleva a los investigadores hasta el límite entre la fe y lo inexplicable.", "conjuring", "HAY PUERTAS QUE NO DEBEN ABRIRSE"),
        new(5, "Atrapado robando", "Crimen · Suspenso", 107, "+16", "Un golpe inesperado desata una carrera por las calles de una ciudad que nunca perdona.", "caught", "CADA DECISIÓN TIENE UN PRECIO"),
        new(6, "La vida de Chuck", "Drama · Fantasía", 110, "+12", "Tres capítulos, una vida y todas las pequeñas cosas que hacen extraordinario lo cotidiano.", "chuck", "CONTENEMOS MULTITUDES"),
        new(7, "Bun y Biggie", "Animación · Aventura", 90, "ATP", "Dos amigos improbables descubren que las mejores aventuras se viven juntos.", "bun", "UNA AMISTAD ENORME")
    });
    public IReadOnlyList<Room> Rooms { get; } = Array.AsReadOnly(new Room[]
    {
        new(1, "Sala 1"), new(2, "Sala 2"), new(3, "Sala 3"), new(4, "Sala 4")
    });

    public InMemoryCinemaService(TimeProvider? clock = null, bool seed = true)
    {
        this.clock = clock ?? TimeProvider.System;
        if (!seed) return;
        // Three days of demonstration programming; the original console seed stays untouched.
        for (var day = 0; day < 3; day++)
        {
            var date = LocalNow.Date.AddDays(day);
            Seed(1, 2, date.AddHours(11).AddMinutes(20), 25);
            Seed(3, 2, date.AddHours(14), 28);
            Seed(1, 2, date.AddHours(17), 25);
            Seed(2, 1, date.AddHours(12).AddMinutes(30), 30);
            Seed(2, 1, date.AddHours(17).AddMinutes(25), 30);
            Seed(4, 3, date.AddHours(11), 35);
            Seed(4, 3, date.AddHours(14).AddMinutes(30), 35);
            Seed(4, 3, date.AddHours(18).AddMinutes(30), 40, "Ultra Láser DOB");
            Seed(5, 4, date.AddHours(11), 30);
            Seed(6, 4, date.AddHours(13).AddMinutes(15), 30);
            Seed(7, 4, date.AddHours(16), 26);
            Seed(3, 4, date.AddHours(19), 28);
        }
    }

    private Screening Seed(int movieId, int roomId, DateTime date, decimal price, string format = "2D DOB")
    {
        var screening = new Screening(Guid.NewGuid(), Movies.Single(m => m.Id == movieId),
            Rooms.Single(r => r.Id == roomId), date, price, format, 0);
        screenings.Add(screening.Id, screening);
        occupied.Add(screening.Id, new(StringComparer.OrdinalIgnoreCase));
        return screening;
    }

    public IReadOnlyList<Screening> GetScreenings(DateOnly? date = null, int? movieId = null, int? roomId = null)
    {
        lock (gate)
            return screenings.Values.Where(s => (!date.HasValue || DateOnly.FromDateTime(s.StartsAt) == date)
                && (!movieId.HasValue || s.Movie.Id == movieId) && (!roomId.HasValue || s.Room.Id == roomId))
                .OrderBy(s => s.StartsAt).ThenBy(s => s.Room.Id).ToArray();
    }

    public Screening? GetScreening(Guid id)
    {
        lock (gate) return screenings.GetValueOrDefault(id);
    }

    public IReadOnlyList<Seat> GetSeats(Guid screeningId)
    {
        lock (gate)
        {
            var screening = RequireScreening(screeningId);
            return Enumerable.Range(0, screening.Room.Rows).SelectMany(row =>
                Enumerable.Range(1, screening.Room.Columns).Select(column =>
                {
                    var label = $"{(char)('A' + row)}{column:00}";
                    return new Seat(label, occupied[screeningId].Contains(label));
                })).ToArray();
        }
    }

    public Booking Reserve(Guid screeningId, IEnumerable<string> seats)
    {
        lock (gate)
        {
            var screening = RequireScreening(screeningId);
            if (screening.StartsAt <= LocalNow) throw new CinemaException("Esta función ya comenzó. Elige otro horario.");
            var selected = seats.Take(9).Select(s => (s ?? "").Trim().ToUpperInvariant()).ToArray();
            if (selected.Length is < 1 or > 8) throw new CinemaException("Selecciona entre 1 y 8 asientos.");
            if (selected.Distinct().Count() != selected.Length) throw new CinemaException("La selección contiene asientos repetidos.");
            var valid = GetSeats(screeningId).Select(s => s.Label).ToHashSet();
            if (selected.Any(s => !valid.Contains(s))) throw new CinemaException("Uno de los asientos no existe en esta sala.");
            if (selected.Any(s => occupied[screeningId].Contains(s)))
                throw new CinemaException("Alguien acaba de reservar uno de estos asientos. Revisa el mapa y elige de nuevo.");
            string code;
            do { code = "SB-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)); } while (bookings.ContainsKey(code));
            var booking = new Booking(code, screeningId, screening.Movie.Title, screening.Room.Name,
                screening.StartsAt, Array.AsReadOnly(selected.Order().ToArray()), screening.Price, LocalNow);
            // Validate everything before changing any seat; one lock makes simultaneous reservations atomic.
            occupied[screeningId].UnionWith(selected);
            screenings[screeningId] = screening with { ReservedSeats = occupied[screeningId].Count };
            bookings.Add(code, booking);
            return booking;
        }
    }

    public Booking? FindBooking(string? code)
    {
        lock (gate) return bookings.GetValueOrDefault((code ?? "").Trim());
    }

    public Booking Cancel(string code)
    {
        lock (gate)
        {
            var booking = FindBooking(code) ?? throw new CinemaException("No encontramos una reserva con ese código.");
            if (booking.Cancelled) throw new CinemaException("Esta reserva ya fue cancelada.");
            if (booking.StartsAt <= LocalNow) throw new CinemaException("No se puede cancelar una función que ya comenzó.");
            occupied[booking.ScreeningId].ExceptWith(booking.Seats);
            var screening = RequireScreening(booking.ScreeningId);
            screenings[screening.Id] = screening with { ReservedSeats = occupied[screening.Id].Count };
            var cancelled = booking with { Cancelled = true };
            bookings[booking.Code] = cancelled;
            return cancelled;
        }
    }

    public Screening AddScreening(int movieId, int roomId, DateTime startsAt, decimal price, string format)
    {
        lock (gate)
        {
            var movie = Movies.FirstOrDefault(m => m.Id == movieId) ?? throw new CinemaException("Selecciona una película válida.");
            if (!Rooms.Any(r => r.Id == roomId)) throw new CinemaException("Selecciona una sala válida.");
            if (startsAt <= LocalNow || startsAt > LocalNow.AddDays(90)) throw new CinemaException("Programa la función dentro de los próximos 90 días.");
            if (price is < 1 or > 500 || decimal.Round(price, 2) != price) throw new CinemaException("El precio debe estar entre Bs 1 y Bs 500, con hasta dos decimales.");
            if (format is not ("2D DOB" or "2D SUB" or "Ultra Láser DOB")) throw new CinemaException("Selecciona un formato válido.");
            var end = startsAt.AddMinutes(movie.DurationMinutes + 15);
            if (screenings.Values.Any(s => s.Room.Id == roomId && startsAt < s.StartsAt.AddMinutes(s.Movie.DurationMinutes + 15) && end > s.StartsAt))
                throw new CinemaException("Ese horario se cruza con otra función en la sala. Incluye 15 minutos de limpieza entre funciones.");
            return Seed(movieId, roomId, DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified), price, format);
        }
    }

    public CinemaReport GetReport(DateOnly? date = null)
    {
        lock (gate)
        {
            var selected = GetScreenings(date);
            var ids = selected.Select(s => s.Id).ToHashSet();
            var active = bookings.Values.Where(b => !b.Cancelled && ids.Contains(b.ScreeningId)).ToArray();
            return new(selected.Count, selected.Sum(s => s.Room.Capacity), selected.Sum(s => s.ReservedSeats),
                active.Length, active.Sum(b => b.Total));
        }
    }

    private Screening RequireScreening(Guid id) => screenings.GetValueOrDefault(id)
        ?? throw new CinemaException("La función no existe.");
}
