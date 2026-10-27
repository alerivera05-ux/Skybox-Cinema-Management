# Sky Box V4 — preparación

Requiere SDK .NET 10 estable. global.json permite versiones de mantenimiento y bandas posteriores de .NET 10; no cambia a .NET 11.

## Estructura

- SkyBox.slnx: solución raíz con Core, Web y consola net10.0.
- src/SkyBox.Core: biblioteca vacía para el dominio futuro.
- src/SkyBox.Web: host ASP.NET Core con Razor Pages registrado y referencia a Core. Sin páginas, CSS ni funciones web; / devuelve 404.
- progam: consola original, fuentes y solución intactos. progam.csproj net6.0 se conserva por compatibilidad histórica, fuera de la nueva solución.

## Verificación y ejecución

```powershell
dotnet restore SkyBox.slnx
dotnet build SkyBox.slnx --no-restore
dotnet build SkyBox.slnx -c Release --no-restore
dotnet run --project progam/SkyBox.csproj
dotnet run --project src/SkyBox.Web/SkyBox.Web.csproj -- --urls http://localhost:5080
```

Elegir siempre el archivo .csproj al ejecutar la consola, porque progam contiene dos proyectos. La solución histórica también se puede compilar con `dotnet build progam/SkyBox.sln`. El proyecto net6.0 requiere sus propios paquetes de referencia y runtime .NET 6; no se cambia su destino ni se compila en paralelo con SkyBox.csproj, ya que comparten obj/bin.

No existen proyectos de pruebas todavía. `dotnet test SkyBox.slnx --no-build` no acredita pruebas funcionales: no descubre ninguna. En esta fase se verifica compilación, arranque de host y salida de consola; las pruebas del dominio se agregarán al extraerlo.

## Decisiones futuras

Diseño de cine premium oscuro. Datos en memoria primero, SQLite después. Esta fase no incorpora almacenamiento, paquetes SQLite, modelos duplicados ni migración de lógica.

La limpieza de bin/obj afecta solamente al seguimiento Git; no borra los archivos físicos de la copia de trabajo ni modifica el contenido histórico del tag. Consultar V4-AUDIT.md para la auditoría previa.
