using System.Collections.Generic;

namespace SkyBox;
public class Cinema
{
    public string Nombre { get; set; } = "Sky Box";
    public List<Sala> Salas { get; } = new();
}