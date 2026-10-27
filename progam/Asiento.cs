namespace SkyBox;
public class Asiento
{
    public char Fila { get; }
    public int Columna { get; }
    public bool Ocupado { get; set; }
    public Asiento(char fila, int columna) { Fila = fila; Columna = columna; Ocupado = false; }
    public override string ToString() => $"{Fila}{Columna:00}";
}