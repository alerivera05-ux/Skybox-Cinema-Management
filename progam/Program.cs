using System;
using System.Globalization;

namespace SkyBox;
class Program
{
    static void Main()
    {
        var cine = DemoData.CrearSkyBox(); 

        while (true)
        {
            Console.WriteLine($"\n=== {cine.Nombre} ===");
            Console.WriteLine("1) Ver cartelera (ordenada por horario - BubbleSort)");
            Console.WriteLine("2) Ver mapa de asientos");
            Console.WriteLine("3) Reservar asiento");
            Console.WriteLine("4) Cancelar boleto");
            Console.WriteLine("5) Reporte de ocupación");
            Console.WriteLine("6) Agregar función a una sala");
            Console.WriteLine("0) Salir");
            Console.Write("Opción: ");
            var op = Console.ReadLine();

            if (op == "0") break;

            switch (op)
            {
                case "1":
                    Cartelera.Mostrar(cine);
                    break;

                case "2":
                    if (Cartelera.ElegirFuncion(cine) is { } f2) f2.MostrarMapa();
                    break;

                case "3":
                    if (Cartelera.ElegirFuncion(cine) is { } f3)
                    {
                        f3.MostrarMapa();
                        Console.Write("Fila (A..K): "); char fila = char.ToUpper(Console.ReadKey().KeyChar); Console.WriteLine();
                        Console.Write("Columna (1..20): "); int col = int.Parse(Console.ReadLine() ?? "1");
                        if (f3.Reservar(fila, col, out string code))
                            Console.WriteLine($"✔ Reserva OK. Código: {code}");
                        else Console.WriteLine("✖ No disponible.");
                    }
                    break;

                case "4":
                    if (Cartelera.ElegirFuncion(cine) is { } f4)
                    {
                        Console.Write("Código de boleto: "); var code = Console.ReadLine() ?? "";
                        Console.WriteLine(f4.Cancelar(code) ? "✔ Cancelado." : "✖ Código no encontrado.");
                    }
                    break;

                case "5":
                    if (Cartelera.ElegirFuncion(cine) is { } f5)
                    {
                        var (oc, li, p) = f5.Ocupacion();
                        Console.WriteLine($"Ocupados: {oc} | Libres: {li} | Total: {oc + li} | Ocupación: {p}%");
                    }
                    break;

                case "6":
                    {
                        var sala = Cartelera.ElegirSala(cine);
                        if (sala == null) break;

                        Console.Write("Título de la película: ");
                        string pelicula = Console.ReadLine() ?? "Sin título";

                        Console.Write("Fecha y hora (dd/MM HH:mm): ");
                        var entrada = Console.ReadLine();
                        if (!DateTime.TryParseExact(entrada, "dd/MM HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime fechaHora))
                        {
                            Console.WriteLine("Formato inválido. Se usará ahora + 2h.");
                            fechaHora = DateTime.Now.AddHours(2);
                        }

                        Console.Write("Precio (Bs): ");
                        decimal precio = decimal.TryParse(Console.ReadLine(), out var p) ? p : 30m;

                        var fun = new Funcion(pelicula, fechaHora, precio, filas: 11, cols: 20);
                        sala.Funciones.Add(fun);
                        sala.OrdenarFuncionesPorHorario_BubbleSort();

                        Console.WriteLine("✔ Función agregada correctamente.");
                        break;
                    }

                default: Console.WriteLine("Opción inválida."); break;
            }
        }
    }
}