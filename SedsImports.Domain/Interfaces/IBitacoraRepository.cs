using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IBitacoraRepository : IRepositorio<MovimientoBitacora>
    {
        /// <summary>Movimientos de un vehículo en orden cronológico (trazabilidad).</summary>
        IReadOnlyList<MovimientoBitacora> ObtenerPorVehiculo(int vehiculoId);

        /// <summary>Movimientos que coinciden con el texto (usuario, rol, vehículo o acción), del más reciente al más antiguo.</summary>
        IReadOnlyList<MovimientoBitacora> Buscar(string? texto);

        /// <summary>Últimos movimientos registrados.</summary>
        IReadOnlyList<MovimientoBitacora> Recientes(int cantidad);
    }
}
