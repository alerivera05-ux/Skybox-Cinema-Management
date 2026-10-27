using System;

namespace SkyBox;
public static class DemoData
{
    public static Cinema CrearSkyBox()
    {
        var cine = new Cinema();

        var s1 = new Sala("Sala 1");
        var s2 = new Sala("Sala 2");
        var s3 = new Sala("Sala 3");
        var s4 = new Sala("Sala 4");

        s1.Funciones.Add(new Funcion("La hora de la desaparición (2D DOB)",  Hoy(12,30), 30));
        s1.Funciones.Add(new Funcion("La hora de la desaparición (2D DOB)",  Hoy(17,25), 30));

        s2.Funciones.Add(new Funcion("Otro viernes de locos (2D DOB)",      Hoy(11,50), 28));
        s2.Funciones.Add(new Funcion("Otro viernes de locos (2D DOB)",      Hoy(16,00), 28));
        s2.Funciones.Add(new Funcion("Superman (2D DOB)", Hoy(11,20), 25));
        s2.Funciones.Add(new Funcion("Superman (2D DOB)", Hoy(15,05), 25));


        s3.Funciones.Add(new Funcion("El Conjuro 4 (2D DOB)",               Hoy(11,00), 35));
        s3.Funciones.Add(new Funcion("El Conjuro 4 (2D DOB)",               Hoy(14,30), 35));
        s3.Funciones.Add(new Funcion("El Conjuro 4 Ultra Láser DOB",        Hoy(18,30), 40));

        s4.Funciones.Add(new Funcion("Atrapado Robando (2D DOB)",           Hoy(11,00), 30));
        s4.Funciones.Add(new Funcion("La vida de Chuck (2D DOB)",           Hoy(13,00), 30));
        s4.Funciones.Add(new Funcion("Bun y Biggie (2D DOB)",               Hoy(13,10), 26));

        cine.Salas.AddRange(new[] { s1, s2, s3, s4 });
        return cine;
    }

    static DateTime Hoy(int h, int m) => DateTime.Today.AddHours(h).AddMinutes(m);
}