# Sky Box V4 — auditoría previa (2026-10-26)

- Rama exclusiva: feature/skybox-v4-web; base 067f28c7edf33946a4aa43d8642798602ff8f3ff.
- Referencias protegidas registradas: main = 067f28c7edf33946a4aa43d8642798602ff8f3ff; tag v3.0-ucatec = 143d07dd187e8dfb7db766a448bdc741577d3836 (objeto de tag).
- Copia de trabajo nueva y limpia. La copia de Desktop no se modifica.
- 66 archivos rastreados: 51 salidas generadas bin/obj y 15 archivos de fuentes, configuración, documentación e imágenes.
- Ocho fuentes C# en progam; namespace SkyBox. Lógica y presentación de consola acopladas en Funcion y Cartelera. No se extraen ni duplican en esta fase.
- progam/SkyBox.csproj apunta a net10.0; progam/progam.csproj conserva net6.0. Ambos incluyen los mismos fuentes y comparten carpetas de salida: usar rutas explícitas y no compilarlos simultáneamente.
- progam/SkyBox.sln incluye solamente SkyBox.csproj; se conserva sin cambios.
- Sin .gitignore, global.json, proyectos de pruebas ni configuración CI en la base. README original conserva instrucciones antiguas; la guía V4 será separada.
- SDK disponible: 10.0.403; runtimes .NET y ASP.NET Core 10.0.14. No hay runtime .NET 6 instalado, por lo que no se promete ejecutar el proyecto histórico net6.0 en este equipo.
- Riesgos: archivos generados versionados, selección ambigua de proyecto dentro de progam y dependencia de consola en el dominio. Se retiran exclusivamente bin/obj del índice, conservándolos físicamente; todos los fuentes y proyectos históricos permanecen intactos.
- Alcance: estructura .NET 10 y host Razor Pages mínimo, sin pantallas de negocio, migración de dominio, reservas web, persistencia ni diseño visual.
