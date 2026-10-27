# Verificación de la versión funcional V4

La implementación continúa exclusivamente en `feature/skybox-v4-web`, sobre la preparación `b5b8d17`. La consola original no se integra como dependencia del dominio web y conserva todos sus fuentes y proyectos.

## Resultados locales

- SDK .NET 10.0.403; runtime ASP.NET Core 10.0.14.
- Compilación de la solución Release: correcta, cero errores y cero advertencias.
- 15/15 pruebas ejecutables del núcleo correctas, incluida una contienda de 100 intentos por el mismo asiento con un único ganador.
- 26/26 verificaciones HTTP correctas con el host en Production. Incluyen creación con precio `30.50`, control de solapamientos, CSRF, reserva de grupo, rechazo atómico de asientos ocupados y liberación tras cancelar.
- Navegador: cartelera, selección de F10/F11, total de Bs 50, confirmación y cancelación verificadas sobre la interfaz real.
- Escritorio y móvil de 390 × 844: portada, filtros, cartelera, mapa y operación revisados visualmente. En móvil el documento mide 375 px útiles por la barra del navegador y no presenta desbordamiento horizontal global; el mapa y la tabla tienen su propio desplazamiento.
- Sin errores ni advertencias en la consola del navegador durante el recorrido verificado.
- Capturas de inicio, móvil, comprobante y operación en `docs/screenshots`.

## Problemas detectados y resueltos

- Formatos decimales: un `<input type=number>` envía punto decimal aunque la cultura de presentación use coma. Se añadió un binder invariante y se verificó el precio exacto por HTTP.
- Tabla móvil: el ancho mínimo y los textos de accesibilidad podían expandir el documento. Se limitó la columna y se posicionó el contenedor de desplazamiento; se volvió a comprobar el ancho real del documento.
- Las recompilaciones de Windows necesitan detener el host de la misma configuración. La compilación final se realiza sin el host que bloquee sus archivos.

## Límites

No se acredita ejecución de .NET 6, pagos, persistencia entre reinicios, autenticación de operadores ni despliegue público. Esta entrega es una demostración funcional local. Los formularios trabajan también sin JavaScript; el cálculo dinámico y la impresión sí lo usan. No se ha certificado accesibilidad completa ni se han probado todos los navegadores.

Las pruebas HTTP verifican el comportamiento real del servidor; las capturas corresponden a Chromium integrado en Codex. El workflow de CI está incluido y su estado remoto se informa por separado tras publicar los commits.
