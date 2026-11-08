using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    public class RolRepository : RepositorioBase<Rol>, IRolRepository
    {
        public RolRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        public override IReadOnlyList<Rol> ObtenerTodos() => Tabla.OrderBy(r => r.Tipo).ToList();

        public Rol? ObtenerPorTipo(TipoRol tipo) => Tabla.FirstOrDefault(r => r.Tipo == tipo);
    }
}
