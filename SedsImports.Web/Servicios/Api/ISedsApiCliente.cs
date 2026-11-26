using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Web.Servicios.Api
{
    /// <summary>
    /// Cliente de la API REST que usa la aplicación web: inicio de sesión (token JWT) y las
    /// funciones que se actualizan sin recargar la página (indicadores, búsqueda y anotaciones).
    /// </summary>
    public interface ISedsApiCliente
    {
        Task<RespuestaApi<TokenDto>> IniciarSesionAsync(string correo, string contrasena, CancellationToken cancelacion = default);

        Task<RespuestaApi<ResumenDto>> ResumenAsync(CancellationToken cancelacion = default);

        Task<RespuestaApi<List<VehiculoListadoDto>>> VehiculosAsync(string? busqueda, EstadoVehiculo? estado, CancellationToken cancelacion = default);

        Task<RespuestaApi<NotaDto>> AgregarNotaAsync(int vehiculoId, string texto, CancellationToken cancelacion = default);
    }
}
