using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SedsImports.Infrastructure.Datos
{
    /// <summary>
    /// Al iniciar la aplicación: aplica las migraciones pendientes (crea la base de datos si no existe)
    /// y carga los datos iniciales si la base está vacía.
    /// </summary>
    public static class InicializadorBaseDatos
    {
        public static void Inicializar(IServiceProvider servicios)
        {
            using var alcance = servicios.CreateScope();
            var contexto = alcance.ServiceProvider.GetRequiredService<SedsImportsDbContext>();
            var log = alcance.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SedsImports.BaseDatos");

            try
            {
                if (contexto.Database.IsRelational() && contexto.Database.GetMigrations().Any())
                    contexto.Database.Migrate();
                else
                    contexto.Database.EnsureCreated();

                DatosIniciales.Cargar(contexto);
                log.LogInformation("Base de datos lista.");
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "No se pudo preparar la base de datos. Revisa la cadena de conexión 'SedsImportsDB' en appsettings.json.");
                throw;
            }
        }
    }
}
