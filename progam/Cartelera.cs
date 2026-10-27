using System;

namespace SkyBox;
public static class Cartelera
{
    public static void Mostrar(Cinema cine)
    {
        Console.WriteLine($"\n=== CARTELERA {cine.Nombre} ===");
        foreach (var sala in cine.Salas)
        {
            sala.OrdenarFuncionesPorHorario_BubbleSort();
            Console.WriteLine($"\n{sala.Nombre}:");
            for (int i = 0; i < sala.Funciones.Count; i++)
            {
                var f = sala.Funciones[i];
                Console.WriteLine($"  [{i}] {f.Pelicula}  {f.FechaHora:dd/MM HH:mm}  Bs {f.Precio}");
            }
        }
    }

    public static Sala? ElegirSala(Cinema cine)
    {
        Console.WriteLine("\nElige sala:");
        for (int s = 0; s < cine.Salas.Count; s++) Console.WriteLine($"[{s}] {cine.Salas[s].Nombre}");
        if (!int.TryParse(Console.ReadLine(), out int isala) || isala < 0 || isala >= cine.Salas.Count) return null;
        return cine.Salas[isala];
    }

    public static Funcion? ElegirFuncion(Cinema cine)
    {
        var sala = ElegirSala(cine);
        if (sala == null) return null;

        sala.OrdenarFuncionesPorHorario_BubbleSort();
        Console.WriteLine("Elige función:");
        for (int i = 0; i < sala.Funciones.Count; i++)
        {
            var f = sala.Funciones[i];
            Console.WriteLine($"[{i}] {f.Pelicula} {f.FechaHora:dd/MM HH:mm}");
        }
        if (!int.TryParse(Console.ReadLine(), out int ifun) || ifun < 0 || ifun >= sala.Funciones.Count) return null;
        return sala.Funciones[ifun];
    }
}