using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Data;
using SedsImports.Web.Repositories;
using SedsImports.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────── MVC ─────────────────────────
builder.Services.AddControllersWithViews(opciones =>
{
    // Protección CSRF en todos los formularios POST (los Tag Helpers agregan el token automáticamente).
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
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
// Repositorios en memoria: Singleton para que los datos vivan mientras corre la aplicación.
// En la Fase 3 se cambian por repositorios con Entity Framework Core (Scoped) sin tocar lo demás.
builder.Services.AddSingleton<IRolRepository, RolRepositoryMemoria>();
builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepositoryMemoria>();
builder.Services.AddSingleton<IVehiculoRepository, VehiculoRepositoryMemoria>();
builder.Services.AddSingleton<IRevisionRepository, RevisionRepositoryMemoria>();
builder.Services.AddSingleton<IDocumentacionRepository, DocumentacionRepositoryMemoria>();
builder.Services.AddSingleton<IClienteRepository, ClienteRepositoryMemoria>();
builder.Services.AddSingleton<IApartadoRepository, ApartadoRepositoryMemoria>();
builder.Services.AddSingleton<IVentaRepository, VentaRepositoryMemoria>();
builder.Services.AddSingleton<IBitacoraRepository, BitacoraRepositoryMemoria>();

// Servicios de negocio: Scoped (una instancia por petición).
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IFotoService, FotoService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IRevisionService, RevisionService>();
builder.Services.AddScoped<IDocumentacionService, DocumentacionService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IApartadoService, ApartadoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualService>();

var app = builder.Build();

// Datos de ejemplo del prototipo.
DatosIniciales.Cargar(app.Services);

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
