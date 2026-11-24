using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;
using SedsImports.Infrastructure.Identidad;
using SedsImports.Infrastructure.Repositorios;

namespace SedsImports.Infrastructure
{
    /// <summary>
    /// Registra todo lo de la capa de infraestructura en el contenedor de dependencias.
    /// Program.cs solo llama a AgregarInfraestructura, sin conocer las clases concretas
    /// (principio de inversión de dependencias).
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, string cadenaConexion)
        {
            services.AddDbContext<SedsImportsDbContext>(opciones =>
                opciones.UseSqlServer(cadenaConexion));

            return services.AgregarRepositorios();
        }

        /// <summary>Repositorios y Unidad de Trabajo: Scoped, comparten el DbContext de la petición.</summary>
        public static IServiceCollection AgregarRepositorios(this IServiceCollection services)
        {
            services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IVehiculoRepository, VehiculoRepository>();
            services.AddScoped<IRevisionRepository, RevisionRepository>();
            services.AddScoped<IDocumentacionRepository, DocumentacionRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IApartadoRepository, ApartadoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IBitacoraRepository, BitacoraRepository>();
            return services;
        }

        /// <summary>
        /// ASP.NET Core Identity: cuentas, roles, política de contraseñas y bloqueo por intentos fallidos.
        /// Usa la misma base de datos, en el esquema "seguridad".
        /// </summary>
        public static IServiceCollection AgregarIdentidad(this IServiceCollection services, string cadenaConexion)
        {
            services.AddDbContext<IdentidadDbContext>(opciones =>
                opciones.UseSqlServer(cadenaConexion, sql =>
                    sql.MigrationsHistoryTable(IdentidadDbContext.TablaHistorial, IdentidadDbContext.Esquema)));

            services.AddIdentityCore<UsuarioIdentidad>(opciones =>
                {
                    // Política de contraseñas: mínimo 8 caracteres con letras y números.
                    opciones.Password.RequiredLength = 8;
                    opciones.Password.RequireDigit = true;
                    opciones.Password.RequireLowercase = false;
                    opciones.Password.RequireUppercase = false;
                    opciones.Password.RequireNonAlphanumeric = false;

                    // Bloqueo: 5 intentos fallidos bloquean la cuenta 15 minutos (fuerza bruta).
                    opciones.Lockout.MaxFailedAccessAttempts = 5;
                    opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    opciones.Lockout.AllowedForNewUsers = true;

                    opciones.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<IdentidadDbContext>();

            services.AddScoped<IServicioIdentidad, ServicioIdentidad>();
            return services;
        }
    }
}
