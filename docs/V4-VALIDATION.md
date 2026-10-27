# Sky Box V4 — resultados de verificación

Fecha local: 2026-10-26. SDK 10.0.403, ASP.NET Core runtime 10.0.14.

| Comprobación | Resultado |
| --- | --- |
| Restauración y build Debug de SkyBox.slnx | Correcto; tres proyectos; cero errores y advertencias |
| Build Release de SkyBox.slnx --no-restore | Correcto; cero errores y advertencias |
| Build de progam/SkyBox.sln --no-restore | Correcto; cero errores y advertencias |
| dotnet test SkyBox.slnx --no-build --no-restore | Salida 0; ningún proyecto de pruebas, ninguna prueba ejecutada |
| Consola Debug: entrada 0 | Menú original visible y salida 0 |
| Host Web Debug en localhost:5080 | Arranca; GET / devuelve HTTP 404, esperado al no existir páginas; proceso detenido al finalizar |
| Fuentes y proyectos originales | Diff vacío para los ocho .cs de progam, ambos .csproj y SkyBox.sln |
| Exclusiones Git | bin/obj nuevos ignorados; progam/Program.cs no ignorado |
| Archivos generados históricos | 51 retirados del índice; archivos físicos conservados |
| git diff --check | Sin errores |

## Límites y observaciones

- El entorno restringido impidió algunas ejecuciones del compilador y el acceso a sockets y claves Data Protection. Se repitieron las verificaciones necesarias con los permisos aprobados y finalizaron correctamente.
- La restauración inicial se ejecutó con -p:NuGetAudit=false para verificar la compilación sin consultar auditoría de paquetes. No hay referencias a paquetes NuGet externos en los proyectos de la nueva solución. No se acredita una auditoría de vulnerabilidades.
- El host vacío informa que no hay descriptores de páginas, como corresponde a esta fase. Al usar HTTP explícito también informa que no puede determinar el puerto HTTPS. Para desarrollo visual posterior habrá que configurar perfiles y HTTPS.
- El proyecto histórico net6.0 se preserva sin modificaciones, pero no se ha compilado ni ejecutado: este equipo carece de su runtime. La compatibilidad de código se conserva; no se afirma soporte operativo de .NET 6 aquí.
- No se probaron reservas ni otras funciones de negocio; no se han modificado. Core todavía no contiene el dominio y Web no lo expone.
- Diseño premium oscuro, persistencia en memoria y posterior SQLite quedan documentados para próximas fases, sin implementación.

## Commits de implementación

1. 4bd690a — auditoría, .gitignore y retirada de salidas generadas del seguimiento.
2. 25be82d — solución .NET 10, Core vacío, host Razor Pages y guía.
3. Este informe — evidencia y límites de validación.

No se fusiona a main ni se actualiza el tag v3.0-ucatec. La publicación se limita al ref feature/skybox-v4-web y se comprueban después las referencias remotas.
