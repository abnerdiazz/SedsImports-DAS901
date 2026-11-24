using System.Text.Json.Serialization;
using SedsImports.Api.Seguridad;
using SedsImports.Application;
using SedsImports.Application.Interfaces;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────── API REST ─────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opciones =>
    {
        // Los enums viajan como texto ("Disponible") y las propiedades en camelCase.
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opciones.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpContextAccessor();

// ───────────────────────── Capas de la solución ─────────────────────────
var cadenaConexion = builder.Configuration.GetConnectionString("SedsImportsDB")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SedsImportsDB'.");

builder.Services.AgregarInfraestructura(cadenaConexion);   // EF Core, repositorios y Unidad de Trabajo
builder.Services.AgregarIdentidad(cadenaConexion);         // ASP.NET Core Identity
builder.Services.AgregarAplicacion();                      // Servicios de negocio
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualApi>();

// ───────────────────────── Seguridad ─────────────────────────
builder.Services.AgregarSeguridadJwt(builder.Configuration);    // JWT + políticas por rol

var app = builder.Build();

// Base de datos del negocio (migraciones de la Fase 3) y tablas de Identity con sus roles.
InicializadorBaseDatos.Inicializar(app.Services);
await InicializadorIdentidad.InicializarAsync(app.Services);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
