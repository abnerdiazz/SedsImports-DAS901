using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>Bitácora: solo se agregan movimientos; las consultas son de solo lectura (AsNoTracking).</summary>
    public class BitacoraRepository : RepositorioBase<MovimientoBitacora>, IBitacoraRepository
    {
        public BitacoraRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        protected override IQueryable<MovimientoBitacora> Consulta() => Tabla.AsNoTracking();

        public IReadOnlyList<MovimientoBitacora> ObtenerPorVehiculo(int vehiculoId) =>
            Consulta().Where(m => m.VehiculoId == vehiculoId)
                      .OrderBy(m => m.Fecha).ThenBy(m => m.Id)
                      .ToList();

        public IReadOnlyList<MovimientoBitacora> Buscar(string? texto)
        {
            var consulta = Consulta();
            if (!string.IsNullOrWhiteSpace(texto))
            {
                var t = texto.Trim();
                consulta = consulta.Where(m =>
                    m.UsuarioNombre.Contains(t) ||
                    m.RolNombre.Contains(t) ||
                    (m.VehiculoNombre != null && m.VehiculoNombre.Contains(t)) ||
                    m.Accion.Contains(t));
            }
            return consulta.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id).ToList();
        }

        public IReadOnlyList<MovimientoBitacora> Recientes(int cantidad) =>
            Consulta().OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
                      .Take(cantidad)
                      .ToList();
    }
}
