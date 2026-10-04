# SED's Imports & Services — Sistema de gestión y seguimiento vehicular

Prototipo web funcional de la **Fase 2 (Desarrollo Web y UX/UI)** de DAS901.
ASP.NET Core MVC (**.NET 10**), patrón MVC con capas de servicios y repositorios, y datos en memoria. La base de datos se integra en la Fase 3.

## Tecnologías

| Tecnología | Uso en el proyecto |
|---|---|
| **C# 14 / .NET 10** | Lenguaje y plataforma de la aplicación |
| **ASP.NET Core MVC** | Patrón Modelo-Vista-Controlador, rutas, filtros e inyección de dependencias |
| **Razor** (Tag Helpers, HTML Helpers, vistas parciales y layouts) | Interfaz web del sistema |
| **DataAnnotations + jQuery Validation Unobtrusive** | Validación en el servidor y en el navegador |
| **HTML5, CSS3 y JavaScript** | Diseño responsive, menú para celular y ventanas modales |
| **Sesión de ASP.NET Core** | Inicio de sesión por perfil (la seguridad formal llega en la Fase 4) |
| **Repositorios en memoria** | Datos de prueba en la Fase 2. En la Fase 3 se reemplazan por Entity Framework Core y SQL Server |
| **Figma** | Wireframes y mockups del prototipo |
| **Git y GitHub** | Control de versiones con una rama por integrante y Pull Requests |

---

## 1. Cómo ejecutarlo

### Requisitos

- **Visual Studio 2026**, o **Visual Studio 2022 versión 17.14 o superior**, con la carga de trabajo *Desarrollo de ASP.NET y web*.
- **SDK de .NET 10**: <https://dotnet.microsoft.com/download/dotnet/10.0>

### Pasos

1. Descomprime el ZIP.
2. Abre **`SedsImports.sln`** con Visual Studio.
3. Presiona **F5** (o *Ctrl + F5*). El navegador se abre en la pantalla de inicio de sesión.
   - La primera vez, Visual Studio puede preguntar si confías en el certificado HTTPS de desarrollo. Responde **Sí**.
   - Si no quieres usar HTTPS, elige el perfil **http** en el botón verde de ejecución.

Desde la terminal también funciona: `dotnet run --project SedsImports.Web`. Después abre <http://localhost:5180>.

> **¿Tienes solo .NET 8?** Cambia `<TargetFramework>net10.0</TargetFramework>` por `net8.0` en `SedsImports.Web/SedsImports.Web.csproj`. El código no usa nada exclusivo de .NET 10. Aun así, el equipo acordó trabajar con **net10.0**.

### Usuarios de prueba

Todos usan la contraseña **`Seds2026`**. En la pantalla de inicio de sesión también hay un botón **"Usar"** que llena las credenciales.

| Usuario | Perfil |
|---|---|
| mario.sanchez@sedimports.com | Encargado de patio |
| jose.hernandez@sedimports.com | Mecánico |
| dania.martinez@sedimports.com | Asistente administrativa |
| ernesto.rivas@sedimports.com | Dueño / vendedor |

> Los datos viven **en memoria**. Al detener la aplicación se reinician a los datos de ejemplo. Esto es intencional en la Fase 2; en la Fase 3 se reemplazan los repositorios por Entity Framework Core.

---

## 2. Qué puede hacer cada perfil

| Acción | Patio | Mecánico | Asistente | Dueño |
|---|:-:|:-:|:-:|:-:|
| Ver panel de inicio propio | ✔ | ✔ | ✔ | ✔ |
| Consultar vehículos (búsqueda + filtros por estado) | ✔ | ✔ (sin apartados ni vendidos) | ✔ | ✔ |
| Registrar vehículo (con fotos de ingreso) | ✔ | | | ✔ |
| **Editar datos de un vehículo ya registrado** | ✔ | | | ✔ |
| Iniciar revisión, agregar anotaciones y fotos | | ✔ | | |
| Finalizar revisión (resumen + **pendientes**) | | ✔ | | |
| Regresar a revisión indicando el motivo | | ✔ | | ✔ |
| **Autorizar que el vehículo quede disponible para venta** | | | | ✔ |
| Actualizar documentación (proceso paralelo) | | | ✔ | ✔ |
| Disponibles, registrar y cancelar apartados | | | | ✔ |
| Registrar venta | | | | ✔ |
| Ventas, reportes, bitácora, trazabilidad y usuarios | | | | ✔ |

Si alguien entra por URL a una sección que no le corresponde, el filtro `[PerfilRequerido]` lo envía a **Acceso denegado**.

---

## 3. Flujo del vehículo y reglas del negocio

```
Recién llegado → En revisión/preparación → Pendiente de autorización → Disponible para venta → Apartado → Vendido
   (patio)            (mecánico)                (espera al dueño)            (dueño)            (dueño)   (dueño)
```

- **Nueva regla:** cuando el mecánico finaliza la revisión, deja su resumen y sus últimos apuntes de lo que le falta al vehículo. El vehículo **no** pasa a disponible automáticamente: queda *Pendiente de autorización*. El dueño ve el resumen y los pendientes, y decide:
  - **Autorizar**: el vehículo queda disponible para venta.
  - **Regresar a revisión**: indica el motivo.
- **Nueva función:** un vehículo registrado se puede **editar** (marca, modelo, año, color, VIN, detalles y fotos nuevas). Lo pueden hacer el encargado de patio y el dueño. Un vehículo vendido ya no se edita. Cada edición queda en la bitácora con los campos que cambiaron.
- El VIN no se puede repetir (CU-01).
- Las anotaciones son obligatorias y solo se registran mientras el vehículo está en revisión (CU-02).
- Si la documentación queda *Pendiente*, se exige indicar qué falta. Para marcarla *Completa* deben estar los 4 documentos principales (CU-04).
- Solo se aparta un vehículo *Disponible* (CU-05). Al apartarlo, deja de aparecer en Disponibles.
- No se puede vender dos veces el mismo vehículo. Si estaba apartado, la venta se registra a nombre del cliente del apartado (CU-06).
- El reporte muestra los 12 meses del año; un período sin ventas aparece en cero (CU-08).
- Toda acción queda en la **bitácora**, y cada vehículo tiene su **trazabilidad** cronológica.
- Un usuario desactivado ya no puede iniciar sesión, y nadie puede desactivarse a sí mismo.

---

## 4. Estructura del proyecto (MVC por capas)

```
SedsImports.sln
SedsImports.Web/
├── Models/          Entidades del dominio (Vehiculo, Revision, Documentacion, Cliente, Apartado, Venta, Rol, Usuario, MovimientoBitacora, Enums)
├── DTOs/            Objetos planos para listados y reportes
├── ViewModels/      Datos que viajan entre controlador y vista, con validaciones (DataAnnotations)
├── Repositories/    Interfaces de acceso a datos + implementación en memoria
├── Data/            Datos iniciales de ejemplo
├── Services/        Lógica y reglas del negocio (una interfaz por responsabilidad)
├── Filters/         [PerfilRequerido]: control de acceso por perfil
├── Helpers/         Nombres visibles de enums, formatos, íconos SVG, hash
├── Controllers/     Acciones, Action Results, rutas y navegación
├── Views/           Razor: layout, vistas por módulo y vistas parciales
├── wwwroot/         CSS (manual de estilos), JS, librerías de validación, fotos subidas
└── Program.cs       Inyección de dependencias, sesión, cultura es-SV y rutas
```

**Recorrido de una petición** (por ejemplo, al registrar un vehículo):
`Vista Create` → `VehiculosController.Create (POST)` → `IVehiculoService.RegistrarAsync` → `IVehiculoRepository.Agregar` → `IBitacoraService.Registrar` → `RedirectToAction("Registrado")`.

### Dónde se ve cada subpunto de la actividad

| Subpunto | Dónde mostrarlo |
|---|---|
| 4.2 Model Binding | Todos los POST reciben ViewModels (`VehiculosController.Create(VehiculoFormViewModel)`). `Ventas/Reporte` hace binding desde el *query string*. |
| 4.3 Validación | `[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]`, `IValidatableObject` (`DocumentacionViewModel`, `FinalizarRevisionViewModel`) y reglas del servicio (VIN duplicado). Funciona en el navegador y en el servidor. |
| 4.5 DTOs | `DTOs/Dtos.cs`: `VehiculoListadoDto`, `ReporteVentasDto`, `VentaMensualDto`, etc. |
| 5.3 Secciones | `@RenderSectionAsync("Scripts")` en `_Layout`; `@section Scripts { ... }` en los formularios. |
| 5.4 HTML Helpers | `Ventas/Reporte` (`Html.DropDownListFor`, `Html.TextBoxFor`, `Html.LabelFor`) y `Html.DisplayNameFor` en los formularios. |
| 5.5 Tag Helpers | `asp-for`, `asp-action`, `asp-route-*`, `asp-validation-for`, `asp-items`, `<partial>`. |
| 5.6 Vistas parciales | `_Mensajes`, `_EstadoBadge`, `_VehiculoFila`, `_DatosCliente`, `_Pasos`, `_Foto`, `_FormApartado`, `_FormVenta`, `_FormDocumentacion`, `Home/_Inicio*`. |
| 6.2 Action Results | `View`, `RedirectToAction`, `Redirect`, `LocalRedirect`, `NotFound`. |
| 6.3 Rutas | Ruta convencional en `Program.cs` y rutas por atributo en `VehiculosController` (`/vehiculos/5/editar`). |
| 6.5 Inyección de dependencias | `Program.cs`: repositorios como Singleton, servicios como Scoped, todo por interfaz. |
| 3.5 Responsive | Menú lateral que se convierte en menú hamburguesa; las tablas se vuelven tarjetas en el celular. |

---

## 5. Equipo y flujo de trabajo

**Integrantes**

- Abner Isaí Díaz Rivera
- Elías Grande Beltrán
- Marlene Yamileth Pérez Alvarado
- Jason Steven Durán López

**Cómo trabajamos con Git**

- `main` contiene la versión estable del proyecto.
- Cada integrante trabaja en su propia rama (`feature/...`) y hace sus propios commits con mensajes descriptivos.
- Al terminar su parte, cada integrante abre un Pull Request hacia `main`, que se revisa antes de fusionarse.
- El trabajo se reparte por capas del patrón MVC: modelos y datos, servicios y controladores, vistas, y diseño UX/UI.

## 6. Notas del prototipo

- El inicio de sesión usa usuario y contraseña, como en el prototipo de Figma.
- Iniciar la revisión, agregar anotaciones y subir fotografías se hace desde la ficha del vehículo. `Revisiones/Editar` es el formulario para finalizar la revisión (resumen y pendientes).
- La Fase 2 no usa base de datos. Entity Framework Core se integra en la Fase 3, y la autenticación y autorización formales en la Fase 4.
