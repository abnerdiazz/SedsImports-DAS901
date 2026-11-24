using System.Globalization;
using System.Text;
using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>Acceso al sistema y administración de usuarios internos (RF-01).</summary>
    public class UsuarioService : ServicioBase, IUsuarioService
    {
        public const string Dominio = "sedimports.com";

        private readonly IUsuarioRepository _usuarios;
        private readonly IRolRepository _roles;
        private readonly IBitacoraService _bitacora;
        private readonly IServicioIdentidad _identidad;

        /// <summary>
        /// Valor que queda en la columna ContrasenaHash de la tabla Usuarios desde la Fase 4:
        /// la contraseña ya no se guarda aquí, sino en la cuenta de ASP.NET Core Identity (hash PBKDF2 con sal).
        /// </summary>
        public const string MarcaIdentity = "IDENTITY";

        public UsuarioService(IUnidadDeTrabajo unidadDeTrabajo, IUsuarioRepository usuarios, IRolRepository roles,
            IBitacoraService bitacora, IServicioIdentidad identidad) : base(unidadDeTrabajo)
        {
            _usuarios = usuarios;
            _roles = roles;
            _bitacora = bitacora;
            _identidad = identidad;
        }

        public async Task<ResultadoOperacion<Usuario>> AutenticarAsync(string correo, string contrasena)
        {
            // Identity valida la contraseña, cuenta los intentos fallidos y bloquea la cuenta si se excede el límite.
            var resultado = await _identidad.AutenticarAsync(correo ?? string.Empty, contrasena ?? string.Empty);
            if (!resultado.Exitoso || resultado.Dato == null) return resultado;

            var usuario = resultado.Dato;
            if (!usuario.Activo)
                return ResultadoOperacion<Usuario>.Error("Tu usuario está desactivado. Consulta con el administrador.");

            return ResultadoOperacion<Usuario>.Ok(usuario, $"Bienvenido, {usuario.PrimerNombre}.");
        }

        public List<Usuario> Listar() => _usuarios.ObtenerTodos().OrderBy(u => u.Rol.Tipo).ThenBy(u => u.NombreCompleto).ToList();

        public Usuario? Obtener(int id) => _usuarios.ObtenerPorId(id);

        public async Task<ResultadoOperacion> CambiarEstadoAsync(int usuarioId, Usuario actor)
        {
            if (!Permisos.PuedeAdministrar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var usuario = _usuarios.ObtenerPorId(usuarioId);
            if (usuario == null) return ResultadoOperacion.Error("El usuario no existe.");
            if (usuario.Id == actor.Id) return ResultadoOperacion.Error("No puedes desactivar tu propio usuario.");

            usuario.Activo = !usuario.Activo;
            _usuarios.Actualizar(usuario);

            var accion = usuario.Activo ? "Activó" : "Desactivó";
            _bitacora.Registrar(actor, null, TipoMovimiento.Usuario, $"{accion} el usuario {usuario.NombreCompleto} ({usuario.Rol.Nombre})");

            var resultado = Guardar($"Usuario {usuario.NombreCompleto} {(usuario.Activo ? "activado" : "desactivado")}.");

            // La cuenta de Identity se bloquea o desbloquea junto con el usuario: un usuario
            // desactivado no puede obtener un token de la API ni iniciar sesión en la Web.
            if (resultado.Exitoso) await _identidad.ActualizarEstadoAsync(usuario);
            return resultado;
        }

        /// <summary>
        /// Roles que se pueden asignar a un usuario nuevo. El dueño / vendedor no aparece:
        /// por decisión del dueño, él es el único administrador del sistema.
        /// </summary>
        public List<Rol> Roles() => _roles.ObtenerTodos()
            .Where(r => r.Tipo != TipoRol.DuenoVendedor)
            .OrderBy(r => r.Tipo)
            .ToList();

        public async Task<ResultadoOperacion<Usuario>> CrearAsync(NuevoUsuarioViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeAdministrar(actor.Rol.Tipo))
                return ResultadoOperacion<Usuario>.Error(Permisos.SinPermiso);

            var rol = modelo.Rol.HasValue ? _roles.ObtenerPorTipo(modelo.Rol.Value) : null;
            if (rol == null)
                return ResultadoOperacion<Usuario>.ErrorCampo(nameof(modelo.Rol), "Selecciona un rol válido.");

            // Regla del negocio: el dueño / vendedor es el único administrador; no se crean otros.
            if (rol.Tipo == TipoRol.DuenoVendedor)
                return ResultadoOperacion<Usuario>.ErrorCampo(nameof(modelo.Rol),
                    "El rol de dueño / vendedor es único y no se puede asignar a otro usuario.");

            var nombre = OrdenarEspacios(modelo.Nombre);
            var apellido = OrdenarEspacios(modelo.Apellido);
            var correo = GenerarCorreo(nombre, apellido);
            if (correo.StartsWith("@"))
                return ResultadoOperacion<Usuario>.ErrorCampo(nameof(modelo.Nombre), "Escribe un nombre y un apellido válidos.");

            var usuario = _usuarios.Agregar(new Usuario
            {
                NombreCompleto = $"{nombre} {apellido}",
                Correo = correo,
                ContrasenaHash = MarcaIdentity,
                Rol = rol,
                Activo = modelo.Activo
            });

            _bitacora.Registrar(actor, null, TipoMovimiento.Usuario,
                $"Creó el usuario {usuario.NombreCompleto} ({rol.Nombre}) · {usuario.Correo} · {(usuario.Activo ? "Activo" : "Inactivo")}");

            var guardado = Guardar(usuario, $"Usuario {usuario.NombreCompleto} creado. Inicia sesión con {usuario.Correo}.");
            if (!guardado.Exitoso) return guardado;

            // Cuenta de acceso en Identity con el rol y la contraseña (Identity aplica su política de contraseñas).
            var cuenta = await _identidad.CrearCuentaAsync(usuario, modelo.Contrasena);
            if (!cuenta.Exitoso)
            {
                // Compensación: si Identity rechaza la cuenta, se quita el usuario para no dejarlo sin acceso.
                _usuarios.Eliminar(usuario);
                UnidadDeTrabajo.GuardarCambios();
                return ResultadoOperacion<Usuario>.ErrorCampo(nameof(modelo.Contrasena), cuenta.Mensaje);
            }

            return guardado;
        }

        public string GenerarCorreo(string? nombre, string? apellido)
        {
            // Primer nombre + primer apellido, sin tildes, sin ñ y en minúsculas: "Lucía Mena" → lucia.mena
            var prefijo = string.Join(".", new[] { PrimeraPalabra(nombre), PrimeraPalabra(apellido) }.Where(p => p.Length > 0));
            var correo = $"{prefijo}@{Dominio}";

            // Si ya existe, se agrega un número: lucia.mena2, lucia.mena3...
            for (var n = 2; _usuarios.ObtenerPorCorreo(correo) != null; n++)
                correo = $"{prefijo}{n}@{Dominio}";

            return correo;
        }

        private static string PrimeraPalabra(string? texto)
        {
            var palabra = OrdenarEspacios(texto).Split(' ')[0];
            var sinTildes = new StringBuilder();
            foreach (var c in palabra.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                var minuscula = char.ToLowerInvariant(c);
                if (minuscula is >= 'a' and <= 'z') sinTildes.Append(minuscula);
            }
            return sinTildes.ToString();
        }

        private static string OrdenarEspacios(string? texto) =>
            string.Join(' ', (texto ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
