using System.Text.Json.Serialization;
using SedsImports.Api.Seguridad;
using SedsImports.Application;
using SedsImports.Application.Interfaces;
using SedsImports.Infrastructure;
using SedsImports.Infrastructure.Datos;

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
builder.Services.AgregarAplicacion();                      // Servicios de negocio
builder.Services.AddScoped<IUsuarioActualService, UsuarioActualApi>();

var app = builder.Build();

// Base de datos del negocio (migraciones de la Fase 3).
InicializadorBaseDatos.Inicializar(app.Services);

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
