using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SedsImports.Api.Configuracion;
using SedsImports.Api.Middleware;
using SedsImports.Api.Seguridad;
using SedsImports.Application;
using SedsImports.Application.Interfaces;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;
using Serilog;

// Logger inicial: registra los errores que ocurran incluso antes de leer la configuración.
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ───────────────────────── Logging (Serilog) ─────────────────────────
    // Consola y archivo diario en logs/ (configuración en appsettings.json, sección "Serilog").
    builder.Host.UseSerilog((contexto, servicios, configuracion) => configuracion
        .ReadFrom.Configuration(contexto.Configuration)
        .ReadFrom.Services(servicios)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Aplicacion", "SedsImports.Api"));

    // ───────────────────────── API REST ─────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opciones =>
        {
            // Los enums viajan como texto ("Disponible") y las propiedades en camelCase.
            opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opciones.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // Errores en formato ProblemDetails (RFC 7807) y manejador global de excepciones.
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ManejadorExcepciones>();
    builder.Services.AddHttpContextAccessor();

    // ───────────────────────── Capas de la solución ─────────────────────────
    var cadenaConexion = builder.Configuration.GetConnectionString("SedsImportsDB")
        ?? throw new InvalidOperationException("Falta la cadena de conexión 'SedsImportsDB'.");

    builder.Services.AgregarInfraestructura(cadenaConexion);   // EF Core, repositorios y Unidad de Trabajo
    builder.Services.AgregarIdentidad(cadenaConexion);         // ASP.NET Core Identity
    builder.Services.AgregarAplicacion();                      // Servicios de negocio
    builder.Services.AddScoped<IUsuarioActualService, UsuarioActualApi>();

    // ───────────────────────── Seguridad, documentación y protección ─────────────────────────
    builder.Services.AgregarSeguridadJwt(builder.Configuration);    // JWT + políticas por rol
    builder.Services.AgregarDocumentacionApi();                     // Swagger
    builder.Services.AgregarProteccion(builder.Configuration);      // Límite de solicitudes, CORS y /health

    var app = builder.Build();

    // Base de datos del negocio (migraciones de la Fase 3) y tablas de Identity con sus roles.
    InicializadorBaseDatos.Inicializar(app.Services);
    await InicializadorIdentidad.InicializarAsync(app.Services);

    // ───────────────────────── Canal de la petición (middleware) ─────────────────────────
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();
    app.UsarEncabezadosSeguridad();
    app.UseSerilogRequestLogging(opciones =>
        opciones.MessageTemplate = "{RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0} ms");

    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Habilitado"))
        app.UsarDocumentacionApi();

    app.UseCors(ProteccionConfiguracion.PoliticaCors);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // GET /health → { estado, duracion, componentes: [...] } (monitoreo del despliegue).
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = (http, reporte) =>
        {
            http.Response.ContentType = "application/json";
            return http.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                estado = reporte.Status.ToString(),
                duracion = reporte.TotalDuration.TotalMilliseconds,
                componentes = reporte.Entries.Select(e => new { nombre = e.Key, estado = e.Value.Status.ToString() })
            }));
        }
    }).DisableRateLimiting();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "La API no pudo iniciar.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Permite que el proyecto de pruebas levante la API en memoria (WebApplicationFactory).</summary>
public partial class Program
{
}
