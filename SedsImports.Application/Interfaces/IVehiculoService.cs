using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IVehiculoService
    {
        List<VehiculoListadoDto> Listar(string? busqueda = null, EstadoVehiculo? estado = null, IEnumerable<EstadoVehiculo>? estadosVisibles = null);
        ConteoEstadosDto Conteos(IEnumerable<EstadoVehiculo>? estadosVisibles = null);
        Vehiculo? Obtener(int id);
        VehiculoListadoDto ADto(Vehiculo vehiculo);
        int RegistradosDesde(DateTime desde);
        Task<ResultadoOperacion<Vehiculo>> RegistrarAsync(VehiculoFormViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> EditarAsync(VehiculoFormViewModel modelo, Usuario actor);
        ResultadoOperacion AutorizarDisponibilidad(int vehiculoId, Usuario actor);
        ResultadoOperacion Eliminar(int vehiculoId, Usuario actor);

        /// <summary>Devuelve el vehículo aunque haya sido eliminado (para consultar su trazabilidad).</summary>
        Vehiculo? ObtenerParaHistorial(int id);

        /// <summary>Ids de los vehículos eliminados (para marcarlos en la bitácora).</summary>
        HashSet<int> IdsEliminados();
    }
}
