using Microsoft.Extensions.DependencyInjection;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Servicios;

namespace SedsImports.Application
{
    /// <summary>
    /// Registra los servicios de negocio en el contenedor de dependencias (Scoped: una instancia
    /// por petición). Lo usan la aplicación web y la API, así las dos aplican las mismas reglas.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
        {
            services.AddScoped<IBitacoraService, BitacoraService>();
            services.AddScoped<IFotoService, FotoService>();
            services.AddScoped<IVehiculoService, VehiculoService>();
            services.AddScoped<IRevisionService, RevisionService>();
            services.AddScoped<IDocumentacionService, DocumentacionService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IApartadoService, ApartadoService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            return services;
        }
    }
}
