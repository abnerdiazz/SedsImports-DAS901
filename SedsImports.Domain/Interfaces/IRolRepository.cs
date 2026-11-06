using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Interfaces
{
    public interface IRolRepository : IRepositorio<Rol>
    {
        Rol? ObtenerPorTipo(TipoRol tipo);
    }
}
