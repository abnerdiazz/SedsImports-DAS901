using SedsImports.Application.Comun;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<ResultadoOperacion<Usuario>> AutenticarAsync(string correo, string contrasena);
        List<Usuario> Listar();
        Usuario? Obtener(int id);
        Task<ResultadoOperacion> CambiarEstadoAsync(int usuarioId, Usuario actor);
        Task<ResultadoOperacion<Usuario>> CrearAsync(NuevoUsuarioViewModel modelo, Usuario actor);

        /// <summary>Roles disponibles para usuarios nuevos (todos menos el dueño / vendedor).</summary>
        List<Rol> Roles();

        /// <summary>Genera el usuario de acceso nombre.apellido@sedimports.com sin repetir uno existente.</summary>
        string GenerarCorreo(string? nombre, string? apellido);
    }
}
