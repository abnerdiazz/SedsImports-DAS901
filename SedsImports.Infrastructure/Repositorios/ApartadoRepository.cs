using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    public class ApartadoRepository : RepositorioBase<Apartado>, IApartadoRepository
    {
        public ApartadoRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        protected override IQueryable<Apartado> Consulta() => Tabla
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo).ThenInclude(v => v.Documentacion);

        public Apartado? ObtenerActivoPorVehiculo(int vehiculoId) =>
            Consulta().FirstOrDefault(a => a.VehiculoId == vehiculoId && a.Estado == EstadoApartado.Activo);

        public IReadOnlyList<Apartado> ObtenerActivos() =>
            Consulta().Where(a => a.Estado == EstadoApartado.Activo)
                      .OrderByDescending(a => a.FechaApartado)
                      .ToList();
    }
}
