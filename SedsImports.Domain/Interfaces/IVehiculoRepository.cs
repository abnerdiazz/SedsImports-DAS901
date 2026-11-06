using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Interfaces
{
    /// <summary>Consultas propias de los vehículos. Los vehículos eliminados (lógicamente) no se devuelven.</summary>
    public interface IVehiculoRepository : IRepositorio<Vehiculo>
    {
        /// <summary>Vehículo con el VIN indicado (para validar que no se repita).</summary>
        Vehiculo? ObtenerPorVin(string vin);

        /// <summary>Busca el vehículo aunque esté eliminado (solo para consultar su historial).</summary>
        Vehiculo? ObtenerIncluyendoEliminados(int id);

        /// <summary>Búsqueda con filtros: texto (marca, modelo, VIN o año), estado y estados que puede ver el perfil.</summary>
        IReadOnlyList<Vehiculo> Buscar(string? texto, EstadoVehiculo? estado, IEnumerable<EstadoVehiculo>? estadosVisibles);

        /// <summary>Vehículos en los estados indicados.</summary>
        IReadOnlyList<Vehiculo> ObtenerPorEstado(params EstadoVehiculo[] estados);

        /// <summary>Cantidad de vehículos agrupados por estado.</summary>
        IReadOnlyDictionary<EstadoVehiculo, int> ContarPorEstado(IEnumerable<EstadoVehiculo>? estadosVisibles);

        /// <summary>Cantidad de vehículos registrados a partir de una fecha.</summary>
        int ContarRegistradosDesde(DateTime desde);

        /// <summary>Ids de los vehículos eliminados lógicamente (para marcarlos en la bitácora).</summary>
        IReadOnlyList<int> IdsEliminados();
    }
}
