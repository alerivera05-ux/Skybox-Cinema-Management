using System.Collections.Generic;

namespace SkyBox;
public class Sala
{
    public string Nombre { get; set; }
    public List<Funcion> Funciones { get; } = new();

    public Sala(string nombre) { Nombre = nombre; }

    // Bubble Sort por FechaHora
    public void OrdenarFuncionesPorHorario_BubbleSort()
    {
        for (int i = 0; i < Funciones.Count - 1; i++)
        for (int j = 0; j < Funciones.Count - i - 1; j++)
            if (Funciones[j].FechaHora > Funciones[j + 1].FechaHora)
            {
                var tmp = Funciones[j];
                Funciones[j] = Funciones[j + 1];
                Funciones[j + 1] = tmp;
            }
    }
}