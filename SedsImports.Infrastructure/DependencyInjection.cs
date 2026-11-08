using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;
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
    }
}
