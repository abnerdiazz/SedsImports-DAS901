using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    public class DocumentacionRepository : RepositorioBase<Documentacion>, IDocumentacionRepository
    {
        public DocumentacionRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        public Documentacion? ObtenerPorVehiculo(int vehiculoId) =>
            Tabla.FirstOrDefault(d => d.VehiculoId == vehiculoId);

        /// <summary>Expedientes pendientes y completos de los vehículos activos que aún no se venden.</summary>
        public (int Pendientes, int Completas) ContarPorEstado()
        {
            var conteo = Tabla
                .Where(d => d.Vehiculo.Estado != EstadoVehiculo.Vendido)
                .GroupBy(d => d.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToList();

            int Cantidad(EstadoDocumentacion e) => conteo.FirstOrDefault(c => c.Estado == e)?.Cantidad ?? 0;
            return (Cantidad(EstadoDocumentacion.Pendiente), Cantidad(EstadoDocumentacion.Completa));
        }
    }
}
