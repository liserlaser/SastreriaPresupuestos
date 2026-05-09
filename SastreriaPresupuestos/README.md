# SastreríaPresupuestos

Aplicación de escritorio desarrollada en **WPF + C# + SQLite + Entity Framework** para la gestión de clientes, presupuestos, productos y entregas en una sastrería o taller de confección a medida.

El proyecto está orientado a un uso práctico en tienda y taller, priorizando:

* rapidez de uso;
* organización clara de clientes y encargos;
* control de entregas;
* generación de documentos PDF;
* flujo táctil para portátil convertible;
* funcionamiento local sin depender de servicios externos.

---

## Estado actual del proyecto

La aplicación se encuentra en una fase **muy avanzada y funcional**, con la reorganización completa por pestañas ya implementada y la mayoría de bloques principales terminados. 

Estado aproximado:

> **85–90% cerrada para uso interno de prueba**. 

Actualmente existe una rama principal de trabajo:

```text
rework-tabs-interface
```

---

# Funcionalidades principales

## Gestión de clientes

* Crear clientes
* Editar clientes
* Eliminar clientes
* Búsqueda rápida
* Teléfono y datos asociados
* Próximas fechas de entrega
* DNI del cliente (en desarrollo/reciente)

---

## Gestión de presupuestos

* Crear presupuestos
* Duplicar presupuestos
* Eliminar presupuestos
* Estados del presupuesto:

  * Pendiente
  * Aceptado
  * Rechazado
  * Entregado
* Fecha de entrega
* Fecha de evento
* Señal / “A cuenta”
* Notas internas
* Notas visibles para cliente
* Avisos de cambios sin guardar

---

## Gestión de productos

* Tabla de líneas independiente
* Añadir productos
* Duplicar líneas
* Eliminar líneas
* Reordenar arriba/abajo
* Control de cantidades
* Control de importes
* Tejido separado como línea específica
* Complementos y extras

---

## Vista de calendario

Implementadas:

* Vista Semana
* Vista Mes
* Navegación anterior / hoy / siguiente
* Filtros por estado
* Filtros por entrega
* Doble clic para abrir cliente/presupuesto

---

## Exportación de documentos

### PDF comercial

Incluye:

* logo;
* datos cliente;
* productos;
* tejido;
* importes;
* total;
* notas cliente;
* numeración;
* pie con IVA.

Las notas internas no aparecen en el PDF comercial. 

---

### Ficha interna de taller

Bloque añadido posteriormente al roadmap inicial:

* exportable solo para presupuestos aceptados;
* incluye cliente, DNI, entrega y observaciones;
* diseñada para impresión y anotaciones manuales.



---

# Estructura de la interfaz

La aplicación utiliza una interfaz reorganizada por pestañas:

* Semana
* Clientes
* Presupuesto
* Productos
* Documentos
* Ajustes



---

# Tecnologías utilizadas

* .NET
* C#
* WPF
* XAML
* SQLite
* Entity Framework Core

---

# Estructura del proyecto

La estructura principal actual incluye:

```text
SastreriaPresupuestos/
│
├── Data/
├── Export/
├── Migrations/
├── Models/
├── Resources/
├── Services/
├── ViewModels/
├── Views/
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── SastreriaPresupuestos.csproj
```



---

# Objetivos del proyecto

El objetivo no es crear un ERP genérico, sino una herramienta rápida y cómoda para el flujo real de trabajo de una sastrería:

1. Registrar clientes rápidamente.
2. Crear presupuestos sin fricción.
3. Organizar entregas.
4. Imprimir documentación clara.
5. Facilitar seguimiento en taller.
6. Mantener toda la información local y simple.

---

# Roadmap resumido

## Completado o muy avanzado

* Rework completo de interfaz
* Sistema por pestañas
* Gestión de clientes
* Gestión de presupuestos
* Gestión de productos
* PDF comercial
* Ficha interna
* Calendario semana/mes
* Ajustes básicos
* Publicación portable



---

## Pendiente / futuro

* Imagen para WhatsApp
* DOCX/XLSX editable
* Datos de empresa configurables
* Productos dinámicos configurables
* Instalador formal
* Mejoras táctiles y responsive



---

# Publicación

La aplicación ya puede publicarse en modo portable para pruebas internas. El siguiente paso recomendado es validar el funcionamiento completo en portátil convertible/táctil. 

---

# Flujo recomendado de pruebas

Prueba funcional recomendada:

1. Crear cliente
2. Guardar cliente
3. Crear presupuesto
4. Añadir productos
5. Añadir tejido
6. Guardar
7. Cerrar y reabrir
8. Revisar calendario
9. Exportar PDFs
10. Validar navegación completa



---

# Requisitos

## Desarrollo

* Visual Studio 2022
* .NET SDK compatible
* Windows 10/11

## Ejecución

* Windows 10/11
* SQLite local

---

# Licencia

Proyecto privado para uso interno y desarrollo personal.