namespace SkyBox;
public class Boleto
{
    public string Codigo { get; }
    public string AsientoLabel { get; }
    public Boleto(string codigo, string asientoLabel) { Codigo = codigo; AsientoLabel = asientoLabel; }
}