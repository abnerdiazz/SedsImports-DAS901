using SedsImports.Domain.Consultas;
using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IRevisionRepository : IRepositorio<Revision>
    {
        /// <summary>Revisión de un vehículo con sus anotaciones.</summary>
        Revision? ObtenerPorVehiculo(int vehiculoId);

        /// <summary>Últimas anotaciones que escribió un usuario, con el vehículo al que pertenecen.</summary>
        IReadOnlyList<NotaConVehiculo> NotasRecientes(int usuarioId, int cantidad);
    }
}
