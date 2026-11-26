# SED's Imports & Services — Sistema de gestión y seguimiento vehicular

Sistema web de la **Fase 4 (Servicios Web, Seguridad y Despliegue)** de DAS901 — proyecto final.
ASP.NET Core (**.NET 10**) con arquitectura por capas (Domain, Application, Infrastructure, **Api** y Web), **Entity Framework Core 10**, **SQL Server LocalDB**, **API REST** con **JWT** y **ASP.NET Core Identity**, documentación con **Swagger**, logging con **Serilog**, pruebas automáticas con **xUnit** y despliegue en **IIS**.

> [!IMPORTANT]
> **🎬 Video demostrativo de la Fase 4:** https://youtu.be/MjtgkQOvVxc
>
> [![Video de la Fase 4 en YouTube](https://img.youtube.com/vi/MjtgkQOvVxc/hqdefault.jpg)](https://youtu.be/MjtgkQOvVxc)

**Videos de las fases anteriores:** [Fase 3 – Persistencia de Datos y Arquitectura Empresarial](https://youtu.be/cvyGGc9cujc)

## Tecnologías

| Tecnología | Uso en el proyecto |
|---|---|
| **C# 14 / .NET 10** | Lenguaje y plataforma |
| **ASP.NET Core MVC** | Aplicación web (vistas Razor, controladores, filtros) |
| **ASP.NET Core Web API** | API REST: controladores `[ApiController]`, GET/POST/PUT/DELETE, respuestas HTTP y ProblemDetails |
| **ASP.NET Core Identity** | Cuentas, contraseñas con hash PBKDF2 + sal, roles y bloqueo por intentos fallidos |
| **JWT (JSON Web Token)** | Autenticación de la API; políticas de autorización por rol |
| **Swagger / OpenAPI (Swashbuckle)** | Documentación interactiva de los endpoints con botón *Authorize* |
| **Fetch API + JSON** | Comunicación asíncrona: búsqueda en vivo, indicadores y anotaciones sin recargar |
| **Serilog** | Logging en consola y archivos diarios (`logs/`) con registro de cada petición |
| **Entity Framework Core 10 (Code First)** | DbContext, Fluent API, migraciones y LINQ |
| **SQL Server Express LocalDB** | Base de datos `SedsImportsDB` (datos del negocio + esquema `seguridad` de Identity) |
| **xUnit + WebApplicationFactory + SQLite** | 33 pruebas de integración de la API |
| **Postman** | Colección de pruebas manuales (`docs/postman`) |
| **IIS** | Publicación de la API y la web (`scripts/publicar-iis.ps1`) |
| **Git y GitHub** | Una rama por integrante y Pull Requests |

---

## 1. Cómo ejecutarlo (desarrollo)

### Requisitos

- **Visual Studio 2026**, o **Visual Studio 2022 17.14 o superior**, con la carga de trabajo *Desarrollo de ASP.NET y web*.
- **SDK de .NET 10**: <https://dotnet.microsoft.com/download/dotnet/10.0>
- **SQL Server Express LocalDB** (viene con la carga de trabajo de ASP.NET y web).
- Opcional: **Postman** para la colección de pruebas.

### Pasos

1. Clona el repositorio y abre **`SedsImports.sln`**.
2. La web ahora **consume la API**, así que se ejecutan los dos proyectos a la vez:
   clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio** → **SedsImports.Api** y **SedsImports.Web** en *Iniciar* (la API primero) → Aceptar.
3. Presiona **F5**. Se abren dos pestañas:
   - **API (Swagger):** <https://localhost:7181/swagger>
   - **Aplicación web:** <https://localhost:7180>
4. La primera vez se crea sola la base `SedsImportsDB` (migraciones `InitialCreate` y `AgregarIdentidad`), se cargan los datos de ejemplo y se crean los roles y las cuentas de Identity.

Desde la terminal: `dotnet run --project SedsImports.Api --launch-profile https` y, en otra terminal, `dotnet run --project SedsImports.Web --launch-profile https`.

### Usuarios de prueba

Todos usan la contraseña **`Seds2026`**. Tras **5 intentos fallidos** la cuenta se bloquea **15 minutos**.

| Usuario | Perfil (rol) |
|---|---|
| mario.sanchez@sedimports.com | Encargado de patio (`EncargadoPatio`) |
| jose.hernandez@sedimports.com | Mecánico (`Mecanico`) |
| dania.martinez@sedimports.com | Asistente administrativa (`AsistenteAdministrativa`) |
| ernesto.rivas@sedimports.com | Dueño / vendedor (`DuenoVendedor`) |

### Clave del token JWT

- En desarrollo está en `SedsImports.Api/appsettings.Development.json`.
- En producción **no se guarda en el repositorio**: se define en la variable de entorno `Jwt__Clave` (el script de IIS genera una aleatoria) o con `dotnet user-secrets set "Jwt:Clave" "..." --project SedsImports.Api`. Si falta o tiene menos de 32 caracteres, la API no inicia.

---

## 2. API REST

Base: `https://localhost:7181/api`. Todas las rutas (menos `login` y `/health`) exigen `Authorization: Bearer {token}`. Los errores usan **ProblemDetails** (RFC 7807).

| Recurso | Endpoints | Quién |
|---|---|---|
| Autenticación | `POST /auth/login` · `GET /auth/perfil` | Todos |
| Vehículos | `GET /vehiculos` · `GET /vehiculos/{id}` · `GET /vehiculos/conteos` · `POST /vehiculos` · `PUT /vehiculos/{id}` · `DELETE /vehiculos/{id}` · `POST /vehiculos/{id}/autorizacion` | Consulta: todos · Alta/edición: patio y dueño · Autorizar/eliminar: dueño |
| Revisión | `GET /vehiculos/{id}/revision` · `POST /vehiculos/{id}/revision` · `GET/POST /vehiculos/{id}/revision/notas` · `PUT /vehiculos/{id}/revision/finalizacion` · `POST /vehiculos/{id}/revision/regreso` | Mecánico (regreso: mecánico y dueño) |
| Documentación | `GET /documentaciones` · `GET/PUT /vehiculos/{id}/documentacion` | Asistente y dueño |
| Apartados | `GET /apartados` · `POST /apartados` · `GET /vehiculos/{id}/apartado` · `POST /vehiculos/{id}/apartado/cancelacion` | Dueño |
| Ventas | `GET /ventas` · `POST /ventas` · `GET /vehiculos/{id}/venta` | Dueño |
| Reportes | `GET /reportes/resumen` · `GET /reportes/ventas?anio=` · `GET /reportes/anios` | Resumen: todos · Ventas: dueño |
| Bitácora | `GET /bitacora?pagina=&tamano=&busqueda=` · `GET /vehiculos/{id}/bitacora` | Dueño |
| Usuarios | `GET /usuarios` · `GET /usuarios/{id}` · `GET /usuarios/{id}/roles` · `POST /usuarios` · `PUT /usuarios/{id}/estado` | Dueño |
| Clientes y catálogos | `GET /clientes` · `GET /clientes/{id}` · `GET /catalogos/{estados-vehiculo, estados-documentacion, roles, tipos-movimiento}` | Clientes: dueño · Catálogos: todos |

**Códigos de respuesta:** 200 OK · 201 Created (con encabezado `Location`) · 204 No Content · 400 datos no válidos (detalle por campo) · 401 sin token o credenciales incorrectas · 403 rol sin permiso · 404 no existe · 409 regla del negocio · 429 demasiadas solicitudes · 500 error interno (con `traceId`).

**Cómo probarla:** Swagger (botón *Authorize*), el archivo `SedsImports.Api/SedsImports.Api.http` en Visual Studio, o la colección `docs/postman/SedsImports-API.postman_collection.json` (el login guarda el token automáticamente).

---

## 3. Seguridad y protección

| Medida | Dónde |
|---|---|
| ASP.NET Core Identity (cuentas, roles, hash PBKDF2 con sal, bloqueo 5 intentos / 15 min, política de contraseñas) | `Infrastructure/Identidad/` |
| JWT firmado HMAC-SHA256 con emisor, audiencia y vencimiento validados | `Api/Seguridad/GeneradorToken.cs`, `ConfiguracionSeguridad.cs` |
| Políticas de autorización por rol | `Api/Seguridad/Politicas.cs` → `[Authorize(Policy = ...)]` |
| Web: autenticación con cookie cifrada, HttpOnly y SameSite=Strict; el token JWT viaja dentro de la cookie (el JavaScript no lo ve) | `Web/Program.cs`, `Web/Controllers/AccesoController.cs`, `ManejadorTokenApi.cs` |
| CSRF: `AutoValidateAntiforgeryToken` en todos los POST, también en las llamadas Fetch (encabezado `X-CSRF-TOKEN`) | `Web/Program.cs`, `_Layout.cshtml`, `wwwroot/js/asincrono.js` |
| XSS: Razor codifica toda la salida; el JavaScript usa `textContent`; Content-Security-Policy sin scripts en línea | `Web/Seguridad/EncabezadosSeguridad.cs` |
| Validación en el servidor (DataAnnotations en DTOs y ViewModels, `[ApiController]` → 400) y en el cliente (jQuery Validation + JavaScript) | `Application/DTOs/Api/`, vistas |
| Límite de solicitudes (10 intentos de login por minuto, 300 peticiones por minuto) | `Api/Configuracion/ProteccionConfiguracion.cs`, `Web/Program.cs` |
| CORS solo para la aplicación web, HTTPS + HSTS, encabezados de seguridad, sin datos sensibles en respuestas ni en el token | `Api/Program.cs`, `Api/Middleware/` |

## 4. Logging, errores y monitoreo

- **Serilog:** consola + archivo diario `logs/api-AAAAMMDD.log` y `logs/web-AAAAMMDD.log` (14 días). Cada petición queda registrada con método, ruta, código y duración.
- **Manejador global de excepciones** (`Api/Middleware/ManejadorExcepciones.cs`): registra el error con su `traceId` y responde ProblemDetails sin mostrar detalles internos.
- **Eventos de seguridad** en el log: inicios de sesión correctos, contraseñas incorrectas y cuentas bloqueadas.
- **Bitácora** del negocio (Fase 3) para la auditoría de cada acción.
- **Salud:** `GET /health` comprueba la API y la conexión con la base de datos.

## 5. Pruebas

```
dotnet test
```

33 pruebas de integración (`SedsImports.Tests`) levantan la API completa en memoria con una base SQLite temporal: autenticación (200/400/401), token alterado, CRUD de vehículos (201/204/400/404/409), autorización por rol (403), flujo completo del registro a la venta, documentación, usuarios, bitácora paginada, catálogos y Swagger.

## 6. Despliegue en IIS

Guía paso a paso en **`docs/DESPLIEGUE.md`**. Resumen (PowerShell como administrador):

```
powershell -ExecutionPolicy Bypass -File .\scripts\publicar-iis.ps1
```

Publica en `C:\inetpub\SedsImports`, crea los sitios **Web → http://localhost:8080** y **API → http://localhost:8081** (Swagger en `/swagger`, salud en `/health`) con su configuración de producción.

---

## 7. Qué puede hacer cada perfil

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
| **Eliminar un vehículo** (eliminación lógica) | | | | ✔ |
| Ventas, reportes, bitácora y trazabilidad | | | | ✔ |
| **Usuarios: agregar, activar y desactivar** | | | | ✔ |

Si alguien entra por URL a una sección que no le corresponde, el filtro `[PerfilRequerido]` lo envía a **Acceso denegado**. En la API, las mismas reglas se aplican con **políticas de autorización** por rol (`[Authorize(Policy = ...)]`): un rol sin permiso recibe **403**.

---

## 8. Flujo del vehículo y reglas del negocio

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
- Un usuario desactivado ya no puede iniciar sesión (su cuenta de Identity queda bloqueada), y nadie puede desactivarse a sí mismo.
- **Alta de usuarios:** el usuario de acceso se genera solo con `nombre.apellido@sedimports.com` (sin tildes ni ñ). Si ya existe, se le agrega un número (`lucia.mena2`). La contraseña debe tener al menos 8 caracteres y combinar letras y números. Solo se pueden crear usuarios de patio, mecánico y asistente: por decisión del dueño, él es el único dueño / vendedor y administrador del sistema.
- **Eliminar vehículo:** solo el dueño, desde la ficha y con confirmación. Es una eliminación lógica: el vehículo desaparece de listados, tableros y búsquedas, pero sus datos y su trazabilidad se conservan, y la eliminación queda registrada en la bitácora con el VIN y el estado que tenía. En la bitácora del dueño, sus movimientos se marcan como **Eliminado** y su trazabilidad queda **solo para consulta**: ya no se puede abrir su ficha ni hacer ninguna acción con él. Un vehículo vendido no se elimina porque forma parte del historial de ventas; si estaba apartado, el apartado se cancela.
- Las acciones importantes (autorizar, desactivar, eliminar) se confirman con ventanas propias del sistema, no con los avisos del navegador.

---

## 9. Arquitectura por capas

```
SedsImports.sln
├── SedsImports.Domain/           Núcleo: entidades, enums, interfaces de repositorios (no depende de nada)
├── SedsImports.Application/      Reglas del negocio → Domain
│   ├── Servicios/ Interfaces/    VehiculoService, UsuarioService (usa IServicioIdentidad), ...
│   ├── DTOs/Api/                 DTOs de entrada y salida de la API con validaciones
│   ├── Mapeos/                   Conversión entidad ↔ DTO ↔ ViewModel (métodos de extensión)
│   └── DependencyInjection.cs    AgregarAplicacion()
├── SedsImports.Infrastructure/   EF Core + Identity → Domain, Application
│   ├── Datos/ Repositorios/      Fase 3 (DbContext, Fluent API, migraciones, repositorios, Unidad de Trabajo)
│   └── Identidad/                IdentidadDbContext (esquema seguridad), ServicioIdentidad, migración AgregarIdentidad
├── SedsImports.Api/              API REST → Application, Infrastructure
│   ├── Controllers/              Auth, Vehiculos, Revisiones, Documentacion, Apartados, Ventas, Reportes, Bitacora, Usuarios, Clientes, Catalogos
│   ├── Seguridad/                JWT, políticas, usuario del token
│   ├── Configuracion/            Swagger, protección (límite de solicitudes, CORS, salud)
│   └── Middleware/               Manejador de excepciones y encabezados de seguridad
├── SedsImports.Web/              MVC → consume la API (HttpClient) y usa las capas de aplicación
│   ├── Servicios/Api/            ISedsApiCliente, ManejadorTokenApi
│   └── Controllers/DatosController.cs + wwwroot/js/asincrono.js   Fetch API
└── SedsImports.Tests/            Pruebas de integración de la API (xUnit)
```

**Recorrido de una petición a la API** (registrar vehículo): `POST /api/vehiculos` + token → `JwtBearer` valida el token → política `GestionPatio` → `[ApiController]` valida `CrearVehiculoDto` → `VehiculosController` → `IVehiculoService.RegistrarAsync` (mismas reglas que la web) → Unidad de Trabajo → `201 Created` con `Location`.

---

## 10. Equipo y flujo de trabajo

- Abner Isaí Díaz Rivera · Elías Grande Beltrán · Marlene Yamileth Pérez Alvarado · Jason Steven Durán López
- `main` tiene la versión estable; cada integrante trabaja en su rama `feature/...` y la integra con un Pull Request.
- **Fase 4:** API REST (Elías), seguridad con Identity y JWT (Marlene), endpoints de operaciones, Swagger y pruebas (Jason), protección, comunicación asíncrona, logging y despliegue (Abner).
