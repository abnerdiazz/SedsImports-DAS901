using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SedsImports.Application.DTOs.Api;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;

namespace SedsImports.Tests
{
    /// <summary>
    /// Levanta la API completa en memoria (mismo Program.cs, mismos controladores y seguridad),
    /// pero con una base de datos SQLite temporal en lugar de SQL Server. Cada ejecución de las
    /// pruebas empieza con los datos iniciales del sistema y no toca la base de datos real.
    /// </summary>
    public class FabricaApi : WebApplicationFactory<Program>
    {
        public const string Contrasena = "Seds2026";
        public const string Dueno = "ernesto.rivas@sedimports.com";
        public const string Patio = "mario.sanchez@sedimports.com";
        public const string Mecanico = "jose.hernandez@sedimports.com";
        public const string Asistente = "dania.martinez@sedimports.com";

        public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly SqliteConnection _conexion = new("DataSource=:memory:");
        private readonly Dictionary<string, string> _tokens = new();

        public FabricaApi()
        {
            // Configuración del entorno de pruebas (se lee al crear la API).
            Environment.SetEnvironmentVariable("Jwt__Clave", "clave-de-pruebas-automaticas-SedsImports-2026");
            Environment.SetEnvironmentVariable("Swagger__Habilitado", "true");
            _conexion.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(servicios =>
            {
                // Se reemplaza SQL Server por SQLite en memoria, compartiendo una sola conexión
                // para que los datos del negocio y las tablas de Identity estén en la misma base.
                Quitar<SedsImportsDbContext>(servicios);
                Quitar<IdentidadDbContext>(servicios);
                servicios.AddDbContext<SedsImportsDbContext>(o => o.UseSqlite(_conexion));
                servicios.AddDbContext<IdentidadDbContext>(o => o.UseSqlite(_conexion));
            });
        }

        private static void Quitar<TContexto>(IServiceCollection servicios) where TContexto : DbContext
        {
            var registros = servicios.Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContexto>) ||
                d.ServiceType == typeof(IDbContextOptionsConfiguration<TContexto>)).ToList();
            foreach (var registro in registros) servicios.Remove(registro);
        }

        /// <summary>Cliente HTTP con el token del usuario indicado (el token se pide una sola vez por usuario).</summary>
        public async Task<HttpClient> ClienteComoAsync(string correo)
        {
            if (!_tokens.TryGetValue(correo, out var token))
            {
                using var anonimo = CreateClient();
                var respuesta = await anonimo.PostAsJsonAsync("/api/auth/login", new LoginDto { Correo = correo, Contrasena = Contrasena });
                respuesta.EnsureSuccessStatusCode();
                token = (await respuesta.Content.ReadFromJsonAsync<TokenDto>(Json))!.Token;
                _tokens[correo] = token;
            }

            var cliente = CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return cliente;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _conexion.Dispose();
        }
    }

    /// <summary>Todas las clases de prueba comparten una sola instancia de la API (más rápido y sin conflictos de logging).</summary>
    [CollectionDefinition(Nombre)]
    public class ColeccionApi : ICollectionFixture<FabricaApi>
    {
        public const string Nombre = "API";
    }
}
