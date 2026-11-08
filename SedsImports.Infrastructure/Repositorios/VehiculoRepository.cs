using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>
    /// Vehículos. El filtro global del DbContext excluye los eliminados en todas las consultas,
    /// excepto en <see cref="ObtenerIncluyendoEliminados"/>.
    /// </summary>
    public class VehiculoRepository : RepositorioBase<Vehiculo>, IVehiculoRepository
    {
        public VehiculoRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        /// <summary>
        /// Carga el vehículo con lo que necesitan los listados y la ficha: fotografías,
        /// documentación y revisión con sus notas. AsSplitQuery evita una sola consulta enorme.
        /// </summary>
        protected override IQueryable<Vehiculo> Consulta() => Tabla
            .Include(v => v.Fotografias)
            .Include(v => v.Documentacion)
            .Include(v => v.Revision!).ThenInclude(r => r.Notas)
            .AsSplitQuery();

        public Vehiculo? ObtenerPorVin(string vin)
        {
            var limpio = vin.Trim().ToUpper();
            return Tabla.FirstOrDefault(v => v.Vin == limpio);
        }

        public Vehiculo? ObtenerIncluyendoEliminados(int id) =>
            Consulta().IgnoreQueryFilters().FirstOrDefault(v => v.Id == id);

        public IReadOnlyList<Vehiculo> Buscar(string? texto, EstadoVehiculo? estado, IEnumerable<EstadoVehiculo>? estadosVisibles)
        {
            var consulta = Consulta();

            if (estadosVisibles != null)
            {
                var visibles = estadosVisibles.ToList();
                consulta = consulta.Where(v => visibles.Contains(v.Estado));
            }

            if (estado.HasValue)
                consulta = consulta.Where(v => v.Estado == estado.Value);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                // LIKE en SQL Server: no distingue mayúsculas (intercalación por defecto).
                var t = texto.Trim();
                var esAnio = int.TryParse(t, out var anio);
                consulta = consulta.Where(v =>
                    v.Marca.Contains(t) ||
                    v.Modelo.Contains(t) ||
                    v.Vin.Contains(t) ||
                    (v.Marca + " " + v.Modelo).Contains(t) ||
                    (esAnio && v.Anio == anio));
            }

            return consulta.OrderByDescending(v => v.FechaIngreso).ToList();
        }

        public IReadOnlyList<Vehiculo> ObtenerPorEstado(params EstadoVehiculo[] estados) =>
            Consulta().Where(v => estados.Contains(v.Estado))
                      .OrderByDescending(v => v.FechaIngreso)
                      .ToList();

        public IReadOnlyDictionary<EstadoVehiculo, int> ContarPorEstado(IEnumerable<EstadoVehiculo>? estadosVisibles)
        {
            var consulta = Tabla.AsQueryable();
            if (estadosVisibles != null)
            {
                var visibles = estadosVisibles.ToList();
                consulta = consulta.Where(v => visibles.Contains(v.Estado));
            }

            // GROUP BY Estado → SELECT Estado, COUNT(*)
            return consulta
                .GroupBy(v => v.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToDictionary(x => x.Estado, x => x.Cantidad);
        }

        public int ContarRegistradosDesde(DateTime desde) => Tabla.Count(v => v.FechaIngreso >= desde);

        public IReadOnlyList<int> IdsEliminados() =>
            Tabla.IgnoreQueryFilters().Where(v => v.Eliminado).Select(v => v.Id).ToList();

        /// <summary>Eliminación lógica: solo marca el vehículo; el filtro global lo oculta.</summary>
        public override void Eliminar(Vehiculo entidad)
        {
            entidad.Eliminado = true;
            entidad.FechaEliminacion ??= DateTime.Now;
            Actualizar(entidad);
        }
    }
}
