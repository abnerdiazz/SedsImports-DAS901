using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Servicios;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;
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

// Sesión: guarda el usuario que inició sesión (la autenticación formal llega en la Fase 4).
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

// Capa de aplicación: servicios de negocio (Scoped, una instancia por petición).
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IFotoService, FotoService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IRevisionService, RevisionService>();
builder.Services.AddScoped<IDocumentacionService, DocumentacionService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IApartadoService, ApartadoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Capa web: usuario de la sesión.
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();

var app = builder.Build();

// Crea o actualiza la base de datos (migraciones) y carga los datos iniciales si está vacía.
InicializadorBaseDatos.Inicializar(app.Services);

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
