using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyBox;
public class Funcion
{
    public string Pelicula { get; set; }
    public DateTime FechaHora { get; set; }
    public decimal Precio { get; set; }

    public List<List<Asiento>> Asientos { get; } = new();
    public List<Boleto> Boletos { get; } = new(); 

    public Funcion(string pelicula, DateTime fechaHora, decimal precio, int filas = 11, int cols = 20)
    {
        Pelicula = pelicula; FechaHora = fechaHora; Precio = precio;
        for (int f = 0; f < filas; f++)
        {
            var fila = new List<Asiento>();
            char letra = (char)('A' + f);
            for (int c = 1; c <= cols; c++) fila.Add(new Asiento(letra, c));
            Asientos.Add(fila);
        }
    }

    public void MostrarMapa()
    {
        Console.WriteLine($"\n[{Pelicula}] {FechaHora:dd/MM HH:mm}  (Bs {Precio})");
        Console.Write("    "); 
        for (int c = 1; c <= Asientos[0].Count; c++) Console.Write($"{c:00} "); 
        Console.WriteLine();
        for (int f = 0; f < Asientos.Count; f++)
        {
            Console.Write($" {Asientos[f][0].Fila}  ");
            for (int c = 0; c < Asientos[f].Count; c++)
                Console.Write(Asientos[f][c].Ocupado ? " X  " : " O  ");
            Console.WriteLine();
        }
    }

    public bool Reservar(char filaChar, int col, out string codigo)
    {
        codigo = "";
        int f = char.ToUpper(filaChar) - 'A';
        int c = col - 1;
        if (f < 0 || f >= Asientos.Count || c < 0 || c >= Asientos[0].Count) return false;
        var a = Asientos[f][c];
        if (a.Ocupado) return false;

        a.Ocupado = true;
        codigo = "B-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        Boletos.Add(new Boleto(codigo, a.ToString()));
        return true;
    }

    public bool Cancelar(string codigo)
    {
        var b = Boletos.FirstOrDefault(x => x.Codigo == codigo);
        if (b == null) return false;

        char fila = b.AsientoLabel[0];
        int col = int.Parse(b.AsientoLabel[1..]);
        var a = Asientos[char.ToUpper(fila) - 'A'][col - 1];
        a.Ocupado = false;
        Boletos.Remove(b);
        return true;
    }

    public (int ocupados, int libres, double porcentaje) Ocupacion()
    {
        int ocup = 0, libres = 0;
        foreach (var fila in Asientos)
            foreach (var a in fila)
                if (a.Ocupado) ocup++; else libres++;
        double p = (ocup + libres) == 0 ? 0 : 100.0 * ocup / (ocup + libres);
        return (ocup, libres, Math.Round(p, 2));
    }
}