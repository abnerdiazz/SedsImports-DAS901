using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IApartadoRepository : IRepositorio<Apartado>
    {
        /// <summary>Apartado activo de un vehículo, con su cliente.</summary>
        Apartado? ObtenerActivoPorVehiculo(int vehiculoId);

        /// <summary>Todos los apartados activos con su vehículo y su cliente.</summary>
        IReadOnlyList<Apartado> ObtenerActivos();
    }
}
