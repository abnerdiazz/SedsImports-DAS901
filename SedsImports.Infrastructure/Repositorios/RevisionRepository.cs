using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Consultas;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    public class RevisionRepository : RepositorioBase<Revision>, IRevisionRepository
    {
        public RevisionRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        protected override IQueryable<Revision> Consulta() => Tabla.Include(r => r.Notas);

        public Revision? ObtenerPorVehiculo(int vehiculoId) =>
            Consulta().FirstOrDefault(r => r.VehiculoId == vehiculoId);

        /// <summary>
        /// Join entre NotasRevision, Revisiones y Vehiculos: últimas anotaciones escritas por el
        /// usuario, sin las automáticas del sistema ni las de vehículos eliminados.
        /// </summary>
        public IReadOnlyList<NotaConVehiculo> NotasRecientes(int usuarioId, int cantidad) =>
            (from nota in Contexto.NotasRevision
             join revision in Contexto.Revisiones on nota.RevisionId equals revision.Id
             join vehiculo in Contexto.Vehiculos on revision.VehiculoId equals vehiculo.Id
             where nota.UsuarioId == usuarioId && !nota.EsDelSistema
             orderby nota.Fecha descending
             select new NotaConVehiculo(
                 vehiculo.Id,
                 vehiculo.Marca + " " + vehiculo.Modelo + " " + vehiculo.Anio,
                 nota.Fecha,
                 nota.Texto))
            .AsNoTracking()
            .Take(cantidad)
            .ToList();
    }
}
