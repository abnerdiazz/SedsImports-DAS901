using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    /// <summary>Usuarios internos. Todas las consultas incluyen el rol del usuario.</summary>
    public interface IUsuarioRepository : IRepositorio<Usuario>
    {
        Usuario? ObtenerPorCorreo(string correo);
    }
}
