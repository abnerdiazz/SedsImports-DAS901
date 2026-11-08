using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IUsuarioActualService
    {
        Usuario? Usuario { get; }
        bool Autenticado { get; }
        TipoRol? Rol { get; }
        void IniciarSesion(Usuario usuario);
        void CerrarSesion();
    }
}
