using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;

namespace SedsImports.Api.Configuracion
{
    /// <summary>
    /// Protección y monitoreo de la API: límite de solicitudes (fuerza bruta en el login),
    /// CORS restringido a la aplicación web y verificación de salud (/health).
    /// </summary>
    public static class ProteccionConfiguracion
    {
        public const string PoliticaLogin = "login";
        public const string PoliticaCors = "web-seds";

        public static IServiceCollection AgregarProteccion(this IServiceCollection services, IConfiguration configuracion)
        {
            // Límite de solicitudes: 10 intentos de inicio de sesión por minuto por dirección IP
            // y 300 solicitudes por minuto para el resto de la API. Al exceder → 429.
            services.AddRateLimiter(opciones =>
            {
                opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                opciones.AddPolicy(PoliticaLogin, http => RateLimitPartition.GetFixedWindowLimiter(
                    http.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
                opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http => RateLimitPartition.GetFixedWindowLimiter(
                    http.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
                opciones.OnRejected = async (contexto, cancelacion) =>
                {
                    contexto.HttpContext.Response.ContentType = "application/problem+json";
                    await contexto.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        status = 429,
                        title = "Demasiadas solicitudes",
                        detail = "Superaste el límite de solicitudes. Espera un minuto e intenta de nuevo."
                    }, cancelacion);
                };
            });

            // CORS: solo la aplicación web del sistema puede llamar a la API desde un navegador.
            var origenes = configuracion.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? Array.Empty<string>();
            services.AddCors(opciones => opciones.AddPolicy(PoliticaCors, p => p
                .WithOrigins(origenes)
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .WithHeaders("Authorization", "Content-Type")));

            // Salud: comprueba que la API responde y que hay conexión con la base de datos.
            services.AddHealthChecks()
                .AddDbContextCheck<SedsImportsDbContext>("base-de-datos", HealthStatus.Unhealthy, new[] { "bd" })
                .AddDbContextCheck<IdentidadDbContext>("identidad", HealthStatus.Unhealthy, new[] { "bd" });

            return services;
        }
    }
}
