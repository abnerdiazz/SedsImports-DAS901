using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Infrastructure.Identidad
{
    /// <summary>
    /// Implementación de las cuentas de acceso con UserManager de ASP.NET Core Identity:
    /// verificación de contraseña, bloqueo por intentos fallidos, creación de cuentas y roles.
    /// </summary>
    public class ServicioIdentidad : IServicioIdentidad
    {
        private const string MensajeCredenciales = "Usuario o contraseña incorrectos.";

        private readonly UserManager<UsuarioIdentidad> _cuentas;
        private readonly IUsuarioRepository _usuarios;
        private readonly ILogger<ServicioIdentidad> _log;

        public ServicioIdentidad(UserManager<UsuarioIdentidad> cuentas, IUsuarioRepository usuarios, ILogger<ServicioIdentidad> log)
        {
            _cuentas = cuentas;
            _usuarios = usuarios;
            _log = log;
        }

        public async Task<ResultadoOperacion<Usuario>> AutenticarAsync(string correo, string contrasena)
        {
            var cuenta = await _cuentas.FindByEmailAsync(correo.Trim());

            // Mismo mensaje si el correo no existe o la contraseña es incorrecta: no revela cuál falló.
            if (cuenta == null)
            {
                _log.LogWarning("Intento de inicio de sesión con un correo no registrado.");
                return ResultadoOperacion<Usuario>.Error(MensajeCredenciales);
            }

            // Un usuario desactivado por el dueño recibe su propio mensaje (no el de bloqueo por intentos).
            var usuarioNegocio = _usuarios.ObtenerPorId(cuenta.UsuarioId);
            if (usuarioNegocio is { Activo: false })
                return ResultadoOperacion<Usuario>.Error("Tu usuario está desactivado. Consulta con el administrador.");

            if (await _cuentas.IsLockedOutAsync(cuenta))
            {
                _log.LogWarning("Inicio de sesión rechazado: la cuenta {CuentaId} está bloqueada.", cuenta.Id);
                return ResultadoOperacion<Usuario>.Error("La cuenta está bloqueada temporalmente por varios intentos fallidos. Intenta de nuevo en unos minutos.");
            }

            if (!await _cuentas.CheckPasswordAsync(cuenta, contrasena))
            {
                // Suma un intento fallido; al llegar al límite configurado, Identity bloquea la cuenta.
                await _cuentas.AccessFailedAsync(cuenta);
                _log.LogWarning("Contraseña incorrecta para la cuenta {CuentaId}. Intentos fallidos: {Intentos}.",
                    cuenta.Id, await _cuentas.GetAccessFailedCountAsync(cuenta));
                return ResultadoOperacion<Usuario>.Error(MensajeCredenciales);
            }

            await _cuentas.ResetAccessFailedCountAsync(cuenta);

            var usuario = usuarioNegocio;
            if (usuario == null) return ResultadoOperacion<Usuario>.Error(MensajeCredenciales);

            _log.LogInformation("Inicio de sesión correcto del usuario {UsuarioId}.", usuario.Id);
            return ResultadoOperacion<Usuario>.Ok(usuario, $"Bienvenido, {usuario.PrimerNombre}.");
        }

        public async Task<ResultadoOperacion> CrearCuentaAsync(Usuario usuario, string contrasena)
        {
            var cuenta = new UsuarioIdentidad
            {
                UserName = usuario.Correo,
                Email = usuario.Correo,
                EmailConfirmed = true,
                UsuarioId = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                LockoutEnabled = true
            };

            var creada = await _cuentas.CreateAsync(cuenta, contrasena);
            if (!creada.Succeeded) return ResultadoOperacion.Error(Describir(creada));

            var rol = await _cuentas.AddToRoleAsync(cuenta, RolesSistema.De(usuario.Rol.Tipo));
            if (!rol.Succeeded) return ResultadoOperacion.Error(Describir(rol));

            await ActualizarEstadoAsync(usuario);
            return ResultadoOperacion.Ok("Cuenta creada.");
        }

        public async Task ActualizarEstadoAsync(Usuario usuario)
        {
            var cuenta = await BuscarPorUsuarioAsync(usuario.Id);
            if (cuenta == null) return;

            // Usuario desactivado = cuenta bloqueada sin fecha de fin. Al activarlo se quita el bloqueo.
            await _cuentas.SetLockoutEndDateAsync(cuenta, usuario.Activo ? null : DateTimeOffset.MaxValue);
            // Cambiar el sello de seguridad invalida las sesiones abiertas de esa cuenta.
            await _cuentas.UpdateSecurityStampAsync(cuenta);
        }

        public async Task<IList<string>> RolesAsync(int usuarioId)
        {
            var cuenta = await BuscarPorUsuarioAsync(usuarioId);
            return cuenta == null ? new List<string>() : await _cuentas.GetRolesAsync(cuenta);
        }

        private Task<UsuarioIdentidad?> BuscarPorUsuarioAsync(int usuarioId) =>
            _cuentas.Users.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        private static string Describir(IdentityResult resultado) =>
            string.Join(" ", resultado.Errors.Select(e => e.Description));
    }
}
