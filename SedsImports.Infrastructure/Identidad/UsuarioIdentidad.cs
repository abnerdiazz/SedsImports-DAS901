using Microsoft.AspNetCore.Identity;

namespace SedsImports.Infrastructure.Identidad
{
    /// <summary>
    /// Cuenta de acceso de ASP.NET Core Identity. Guarda el correo, el hash de la contraseña
    /// (PBKDF2 con sal), el sello de seguridad, los intentos fallidos y el bloqueo.
    /// Se enlaza 1 a 1 con el usuario del negocio (tabla Usuarios) mediante UsuarioId.
    /// </summary>
    public class UsuarioIdentidad : IdentityUser<int>
    {
        /// <summary>Id del usuario en la tabla Usuarios (datos del negocio: nombre, rol, estado).</summary>
        public int UsuarioId { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;
    }
}
