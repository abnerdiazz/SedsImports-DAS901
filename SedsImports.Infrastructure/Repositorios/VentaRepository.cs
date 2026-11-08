using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Consultas;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>Ventas y consultas del reporte (agrupaciones y promedios con LINQ).</summary>
    public class VentaRepository : RepositorioBase<Venta>, IVentaRepository
    {
        public VentaRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        protected override IQueryable<Venta> Consulta() => Tabla
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo);

        public Venta? ObtenerPorVehiculo(int vehiculoId) =>
            Consulta().FirstOrDefault(v => v.VehiculoId == vehiculoId);

        /// <summary>Ventas con su vehículo y su cliente (INNER JOIN), de la más reciente a la más antigua.</summary>
        public IReadOnlyList<Venta> ObtenerConDetalle(int? anio = null)
        {
            var consulta = Consulta().AsNoTracking();
            if (anio.HasValue)
                consulta = consulta.Where(v => v.FechaVenta.Year == anio.Value);

            return consulta
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.FechaRegistro)
                .ToList();
        }

        /// <summary>GROUP BY mes: unidades y total vendido de cada mes del año.</summary>
        public IReadOnlyList<ResumenVentasMes> ResumenMensual(int anio) =>
            Tabla.Where(v => v.FechaVenta.Year == anio)
                 .GroupBy(v => v.FechaVenta.Month)
                 .OrderBy(g => g.Key)
                 .Select(g => new ResumenVentasMes(g.Key, g.Count(), g.Sum(v => v.Monto ?? 0)))
                 .ToList();

        /// <summary>
        /// Consulta avanzada: por marca, unidades vendidas, total y días promedio que el vehículo
        /// estuvo en el patio (desde su ingreso hasta la venta). Se ordena por unidades vendidas.
        /// </summary>
        public IReadOnlyList<ResumenVentasMarca> ResumenPorMarca(int anio)
        {
            var datos = (from venta in Tabla
                         join vehiculo in Contexto.Vehiculos on venta.VehiculoId equals vehiculo.Id
                         where venta.FechaVenta.Year == anio
                         select new { vehiculo.Marca, venta.Monto, vehiculo.FechaIngreso, venta.FechaVenta })
                        .AsNoTracking()
                        .ToList();

            // Los días en el patio se calculan en memoria para que la consulta funcione en cualquier proveedor.
            return datos
                .GroupBy(d => d.Marca)
                .Select(g => new ResumenVentasMarca(
                    g.Key,
                    g.Count(),
                    g.Sum(d => d.Monto ?? 0),
                    Math.Round(g.Average(d => (d.FechaVenta - d.FechaIngreso.Date).TotalDays), 1)))
                .OrderByDescending(r => r.Cantidad)
                .ThenByDescending(r => r.Total)
                .ToList();
        }

        public IReadOnlyList<int> AniosConVentas() =>
            Tabla.Select(v => v.FechaVenta.Year)
                 .Distinct()
                 .OrderByDescending(a => a)
                 .ToList();

        public (int Cantidad, decimal Total) TotalesDelMes(int anio, int mes)
        {
            var delMes = Tabla.Where(v => v.FechaVenta.Year == anio && v.FechaVenta.Month == mes);
            return (delMes.Count(), delMes.Sum(v => v.Monto ?? 0));
        }
    }
}
