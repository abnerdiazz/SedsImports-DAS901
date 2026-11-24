using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Application;
using SedsImports.Application.Interfaces;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;
using SedsImports.Web.Filters;
using SedsImports.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────── MVC ─────────────────────────
builder.Services.AddControllersWithViews(opciones =>
{
    // Protección CSRF en todos los formularios POST (los Tag Helpers agregan el token automáticamente).
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Errores de base de datos: mensaje claro al usuario en lugar de una pantalla de error.
    opciones.Filters.Add<ErrorPersistenciaFilter>();
});

// Sesión: guarda el usuario que inició sesión (las credenciales se validan con ASP.NET Core Identity).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.Cookie.Name = ".SedsImports.Sesion";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddHttpContextAccessor();

// ───────────────────────── Inyección de dependencias ─────────────────────────
// Capa de infraestructura: DbContext (SQL Server), repositorios y Unidad de Trabajo (Scoped).
var cadenaConexion = builder.Configuration.GetConnectionString("SedsImportsDB")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SedsImportsDB' en appsettings.json.");
builder.Services.AgregarInfraestructura(cadenaConexion);

// ASP.NET Core Identity: cuentas de acceso, roles y bloqueo por intentos fallidos (esquema seguridad).
builder.Services.AgregarIdentidad(cadenaConexion);

// Capa de aplicación: servicios de negocio (Scoped). Se registran con AgregarAplicacion(),
// el mismo método que usa la API, para que las dos apliquen las mismas reglas.
builder.Services.AgregarAplicacion();

// Capa web: usuario de la sesión.
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();

var app = builder.Build();

// Crea o actualiza la base de datos (migraciones) y carga los datos iniciales si está vacía.
InicializadorBaseDatos.Inicializar(app.Services);
await InicializadorIdentidad.InicializarAsync(app.Services);

// Cultura de El Salvador para fechas y números.
var cultura = new CultureInfo("es-SV");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Páginas de error amigables para 404, 403, etc.
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Ruta convencional: la página inicial es el inicio de sesión (Acceso/Index).
// VehiculosController además usa rutas por atributo (/vehiculos/...).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Index}/{id?}");

app.Run();
