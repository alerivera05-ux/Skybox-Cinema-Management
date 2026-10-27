#  Sky Box - Sistema de Gestión de Cine

![Portada Cine](img/portada.jpg)


---

##  Índice
- [Descripción](#-descripción)
- [Instalación y ejecución](#-instalación-y-ejecución)
- [Estructura de clases](#-estructura-de-clases)
- [Algoritmo de ordenamiento](#-algoritmo-de-ordenamiento)
- [Aplicación en el cine](#-aplicación-en-el-cine)
- [Autores](#-autores)

---

##  Descripción
Este sistema simula la gestión de un cine llamado **Sky Box**, permitiendo:
- Ver la cartelera ordenada por horario.  
- Mostrar el mapa de asientos de cada función.  
- Reservar y cancelar boletos con código único.  
- Generar reportes de ocupación.  
- Agregar nuevas funciones a una sala.  

El proyecto está implementado en **C# con Programación Orientada a Objetos (POO)** y hace uso de **listas anidadas** y el algoritmo de ordenamiento **Bubble Sort**.

---

##  Instalación y ejecución
1. Clonar el repositorio:
   ```bash
   git clone https://github.com/RaulOzk4R/Grupo2-DemostracionCompetencias/tree/cine-v3(skybox)
   cd skybox
   ```
2. Compilar y ejecutar:
   ```bash
   dotnet build
   dotnet run
   ```

---

##  Estructura de clases
![Diagrama de Clases](img/readm.png)


> El cine se modela con un diseño jerárquico:  
> **Cinema → Sala → Funciones → Asientos/Boletos**.  
> Cada parte está implementada como una clase independiente, aplicando los principios de POO.

---

##  Algoritmo de ordenamiento
El sistema utiliza el algoritmo **Bubble Sort** para ordenar las funciones de cada sala por horario.  
Esto asegura que la cartelera siempre se muestre de forma **cronológica**, incluso si las funciones se agregan en desorden.

---

##  Aplicación en el cine
Este sistema aplica al cine de la siguiente manera:
- Cada **sala** se modela con una lista de funciones.  
- Cada **función** se gestiona con un mapa de asientos (listas de listas).  
- Al reservar un asiento se genera un **boleto con código único**.  
- El cine puede **agregar nuevas películas y funciones** dinámicamente.  

---

##  Miembros
- Colque Viscarra Jhair Abrahan  
- Lazarte Mancilla Raúl Oscar  
- Rivera Villarroel Marcelo Alejandro  
