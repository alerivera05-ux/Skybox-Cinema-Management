# Sky Box V4 — versión web funcional

Aplicación local ASP.NET Core Razor Pages sobre .NET 10. Diseño oscuro, acento verde lima y cartelera con arte gráfico propio, sin dependencias de imágenes ni fuentes externas. Esta versión implementa el recorrido completo de la consola en la web y preserva `progam`.

## Iniciar en Windows

Desde la carpeta del repositorio, con SDK .NET 10 instalado:

```powershell
dotnet restore SkyBox.slnx
dotnet build SkyBox.slnx -c Release --no-restore
dotnet run --project src/SkyBox.Web --no-launch-profile -c Release --no-build -- --urls http://localhost:5080
```

Abrir http://localhost:5080. Para detener el servidor, Ctrl+C. El perfil de desarrollo también permite `dotnet run --project src/SkyBox.Web` en el mismo puerto. Detener la aplicación antes de recompilar en Windows, porque bloquea sus DLL.

## Recorrido de demostración

1. En Cartelera, elegir fecha, película o sala. Si terminaron las funciones de hoy, la portada selecciona mañana automáticamente.
2. Abrir un horario disponible. El mapa tiene 11 filas (A–K) y 20 columnas. En móvil se desplaza horizontalmente dentro del panel.
3. Seleccionar de uno a ocho asientos. El resumen actualiza el importe; confirmar genera un código impredecible `SB-…`. El servidor vuelve a comprobar disponibilidad y precio.
4. Guardar o imprimir el comprobante. El navegador ofrece imprimir o guardar como PDF. La reserva no implica pago: el importe está pendiente en boletería.
5. En Mi reserva, ingresar el código. Se puede consultar y cancelar el grupo completo antes de que empiece la función. Cancelar libera todos sus asientos.
6. En Operación, consultar funciones, ocupación, reservas activas e importe reservado para una fecha. Crear una función seleccionando película, sala, fecha/hora, formato y precio.
7. Una función nueva aparece en cartelera al elegir su fecha. No puede solaparse con otra en la sala; se reservan también 15 minutos de limpieza.

Hay siete películas y cuatro salas de demostración. Se generan 12 funciones diarias durante tres días al iniciar. Los títulos y descripciones forman parte del catálogo demo; las composiciones gráficas son propias y no son afiches oficiales.

## Reglas y decisiones

- Horarios de Bolivia, UTC−4, independientes de la zona horaria del servidor.
- Una sola instancia de cine en memoria para todos los visitantes del servidor.
- Reservas atómicas: si falla un asiento, no se reserva ninguno del grupo. Un bloqueo sincroniza las escrituras y lecturas relevantes.
- Los asientos seleccionados no se retienen mientras se navega: quedan confirmados al enviar la reserva. Una selección desactualizada muestra un error y el mapa actual.
- Reserva y cancelación cerradas desde el comienzo de la función.
- Precio exacto con `decimal`, entre Bs 1 y Bs 500 y hasta dos decimales. La programación admite los próximos 90 días.
- Los códigos permiten gestionar la reserva sin datos personales. No se deben compartir fuera del grupo.
- Todo vuelve al catálogo inicial al reiniciar. SQLite sigue previsto para la próxima fase; `ICinemaService` separa el almacenamiento de las páginas.
- No hay pagos, autenticación, correos, afiches descargados ni despliegue público. Operación está abierta para la demostración local. Una publicación real deberá incorporar acceso de operadores, almacenamiento persistente y HTTPS.

## Compilar y probar

```powershell
dotnet build SkyBox.slnx -c Release
dotnet run --project tests/SkyBox.Core.Tests -c Release --no-build
pwsh -File scripts/Smoke-Web.ps1
```

El primer ejecutable corre 15 pruebas del núcleo, incluidas concurrencia, conflictos de grupos, cancelación, horarios, límites e importes. Sale con código distinto de cero si falla alguna. Se eligió un ejecutable de pruebas sin paquetes adicionales; **no se ejecuta con `dotnet test`**.

El script PowerShell corre 26 verificaciones HTTP contra un servidor Production temporal en el puerto 5081: renderizado, filtros, recursos, protección CSRF, reservas, conflictos, comprobantes, cancelaciones, creación de funciones y precio decimal. Lo detiene siempre al terminar y conserva su log en `.dotnet-local/http-tests.log` (ignorado). Usar `-Port 5082` si 5081 está ocupado. Requiere PowerShell 7 y compilación Release previa.

El workflow `.github/workflows/skybox-v4.yml` repite compilación, pruebas del núcleo y HTTP en Ubuntu para cada push de la rama de trabajo y pull request a las ramas indicadas.

La consola histórica se ejecuta con `dotnet run --project progam/SkyBox.csproj`. No ejecutar simultáneamente los dos proyectos históricos: comparten obj/bin. `progam/progam.csproj` conserva .NET 6 y no participa en la solución V4.

## Estructura

```text
SkyBox.slnx
src/SkyBox.Core/         Modelos, interfaz y almacenamiento en memoria
src/SkyBox.Web/          Razor Pages, estilos, JavaScript y perfiles locales
tests/SkyBox.Core.Tests/ Pruebas ejecutables sin paquetes externos
scripts/Smoke-Web.ps1    Pruebas HTTP de principio a fin
progam/                 Consola original intacta
docs/screenshots/       Capturas de escritorio, móvil y comprobante
```

Los informes `V4-AUDIT`, `V4-PREPARATION` y `V4-VALIDATION` describen la fase inicial. Este documento y `V4-FUNCTIONAL-VALIDATION` describen la versión funcional posterior.
