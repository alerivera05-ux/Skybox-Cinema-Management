# SkyBox.Core

Biblioteca net10.0 independiente de Razor Pages y de la consola. Contiene el catálogo de demostración, programación, selección de asientos, reservas de grupos, cancelaciones y reportes.

`ICinemaService` define la frontera de almacenamiento. `InMemoryCinemaService` mantiene una única instancia por servidor, aplica validaciones antes de modificar el estado y sincroniza las operaciones para impedir reservas dobles. Los modelos devueltos son registros con colecciones de solo lectura o copias.

La versión histórica permanece en `progam` sin cambios. La nueva implementación adapta su comportamiento al entorno web sin modificar el ejecutable original. SQLite se incorporará en una fase posterior mediante la misma interfaz.
