using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SedsImports.Application;
using SedsImports.Application.Interfaces;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;
using SedsImports.Web.Filters;
using SedsImports.Web.Seguridad;
using SedsImports.Web.Servicios;
using SedsImports.Web.Servicios.Api;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ───────────────────────── Logging (Serilog) ─────────────────────────
    builder.Host.UseSerilog((contexto, servicios, configuracion) => configuracion
        .ReadFrom.Configuration(contexto.Configuration)
        .ReadFrom.Services(servicios)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Aplicacion", "SedsImports.Web"));

    // ───────────────────────── MVC ─────────────────────────
    builder.Services.AddControllersWithViews(opciones =>
    {
        // Protección CSRF en todos los POST: formularios (Tag Helpers) y llamadas Fetch (encabezado X-CSRF-TOKEN).
        opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

        // Errores de base de datos: mensaje claro al usuario en lugar de una pantalla de error.
        opciones.Filters.Add<ErrorPersistenciaFilter>();
    });

    builder.Services.AddAntiforgery(opciones =>
    {
        opciones.HeaderName = "X-CSRF-TOKEN";
        opciones.Cookie.Name = ".SedsImports.Antiforgery";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Strict;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    // ───────────────────────── Autenticación con cookie ─────────────────────────
    // La cookie guarda los claims del usuario y, cifrado, el token JWT que entregó la API.
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(opciones =>
        {
            opciones.Cookie.Name = ".SedsImports.Auth";
            opciones.Cookie.HttpOnly = true;                       // El JavaScript no puede leerla (XSS).
            opciones.Cookie.SameSite = SameSiteMode.Strict;        // No se envía desde otros sitios (CSRF).
            opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            opciones.LoginPath = "/Acceso";
            opciones.LogoutPath = "/Acceso/Salir";
            opciones.AccessDeniedPath = "/Acceso/Denegado";
            opciones.SlidingExpiration = false;
        });
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();

    // Límite de intentos de inicio de sesión desde la web: 10 por minuto por dirección IP.
    builder.Services.AddRateLimiter(opciones =>
    {
        opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opciones.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    });

    // ───────────────────────── Cliente de la API REST ─────────────────────────
    builder.Services.AddOptions<ApiOpciones>()
        .Bind(builder.Configuration.GetSection(ApiOpciones.Seccion))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddTransient<ManejadorTokenApi>();
    builder.Services.AddHttpClient<ISedsApiCliente, SedsApiCliente>((servicios, cliente) =>
        {
            var api = servicios.GetRequiredService<IOptions<ApiOpciones>>().Value;
            cliente.BaseAddress = new Uri(api.UrlBase.TrimEnd('/') + "/");
            cliente.Timeout = TimeSpan.FromSeconds(api.SegundosEspera);
        })
        .AddHttpMessageHandler<ManejadorTokenApi>();

    // ───────────────────────── Capas de la solución ─────────────────────────
    var cadenaConexion = builder.Configuration.GetConnectionString("SedsImportsDB")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'SedsImportsDB' en appsettings.json.");
    builder.Services.AgregarInfraestructura(cadenaConexion);
    builder.Services.AgregarIdentidad(cadenaConexion);
    builder.Services.AgregarAplicacion();
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
    app.UsarEncabezadosSeguridad();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging(opciones =>
        opciones.MessageTemplate = "{RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0} ms");
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Ruta convencional: la página inicial es el inicio de sesión (Acceso/Index).
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Acceso}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "La aplicación web no pudo iniciar.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
