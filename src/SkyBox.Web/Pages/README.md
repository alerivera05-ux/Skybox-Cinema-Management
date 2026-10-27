# Pages

Razor Pages de Sky Box V4:

- `/`: cartelera, filtros de fecha, película y sala.
- `/funcion/{id}`: mapa de 220 asientos y reserva de hasta ocho entradas.
- `/reserva/{code}`: comprobante imprimible y estado de la reserva.
- `/mis-reservas`: consulta y cancelación por código.
- `/operacion`: ocupación, importes reservados y creación de funciones.

Las páginas usan `ICinemaService`; el cálculo de precios y todas las reglas se verifican en el servidor. Los formularios POST llevan protección antifalsificación de Razor Pages.
