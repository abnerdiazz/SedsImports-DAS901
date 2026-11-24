using SedsImports.Application.Comun;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    /// <summary>
    /// Cuentas de acceso del sistema (ASP.NET Core Identity). La capa de aplicación solo conoce
    /// esta interfaz; la implementación con UserManager vive en la infraestructura
    /// (principio de inversión de dependencias).
    /// </summary>
    public interface IServicioIdentidad
    {
        /// <summary>
        /// Verifica correo y contraseña con Identity. Cuenta los intentos fallidos y bloquea
        /// la cuenta temporalmente después de varios intentos (protección contra fuerza bruta).
        /// </summary>
        Task<ResultadoOperacion<Usuario>> AutenticarAsync(string correo, string contrasena);

        /// <summary>Crea la cuenta de Identity del usuario con su rol y contraseña (Identity guarda solo el hash con sal).</summary>
        Task<ResultadoOperacion> CrearCuentaAsync(Usuario usuario, string contrasena);

        /// <summary>Activa o bloquea la cuenta de Identity según el estado del usuario.</summary>
        Task ActualizarEstadoAsync(Usuario usuario);

        /// <summary>Roles de Identity asignados a la cuenta del usuario.</summary>
        Task<IList<string>> RolesAsync(int usuarioId);
    }
}
