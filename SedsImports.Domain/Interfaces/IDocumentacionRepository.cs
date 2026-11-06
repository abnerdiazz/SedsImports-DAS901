using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IDocumentacionRepository : IRepositorio<Documentacion>
    {
        /// <summary>Expediente de documentos de un vehículo.</summary>
        Documentacion? ObtenerPorVehiculo(int vehiculoId);

        /// <summary>Cantidad de expedientes pendientes y completos de los vehículos que aún no se venden.</summary>
        (int Pendientes, int Completas) ContarPorEstado();
    }
}
