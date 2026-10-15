using System.Globalization;
using System.Text;
using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>Acceso al sistema y administración de usuarios internos (RF-01).</summary>
    public class UsuarioService : IUsuarioService
    {
        public const string Dominio = "sedimports.com";

        private readonly IUsuarioRepository _usuarios;
        private readonly IRolRepository _roles;
        private readonly IBitacoraService _bitacora;

        public UsuarioService(IUsuarioRepository usuarios, IRolRepository roles, IBitacoraService bitacora)
        {
            _usuarios = usuarios;
            _roles = roles;
            _bitacora = bitacora;
        }

        public ResultadoOperacion<Usuario> Autenticar(string correo, string contrasena)
        {
            var usuario = _usuarios.ObtenerPorCorreo(correo ?? string.Empty);

            // Mismo mensaje para usuario o contraseña incorrectos: no revela cuál de los dos falló.
            if (usuario == null || usuario.ContrasenaHash != Seguridad.Hash(contrasena ?? string.Empty))
                return ResultadoOperacion<Usuario>.Error("Usuario o contraseña incorrectos.");

            if (!usuario.Activo)
                return ResultadoOperacion<Usuario>.Error("Tu usuario está desactivado. Consulta con el administrador.");

            return ResultadoOperacion<Usuario>.Ok(usuario, $"Bienvenido, {usuario.PrimerNombre}.");
        }

        public List<Usuario> Listar() => _usuarios.ObtenerTodos().OrderBy(u => u.Rol.Tipo).ThenBy(u => u.NombreCompleto).ToList();

        public Usuario? Obtener(int id) => _usuarios.ObtenerPorId(id);

        public ResultadoOperacion CambiarEstado(int usuarioId, Usuario actor)
        {
            if (!Permisos.PuedeAdministrar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var usuario = _usuarios.ObtenerPorId(usuarioId);
            if (usuario == null) return ResultadoOperacion.Error("El usuario no existe.");
            if (usuario.Id == actor.Id) return ResultadoOperacion.Error("No puedes desactivar tu propio usuario.");

            usuario.Activo = !usuario.Activo;
            _usuarios.Actualizar(usuario);

            var accion = usuario.Activo ? "Activó" : "Desactivó";
            _bitacora.Registrar(actor, null, TipoMovimiento.Usuario, $"{accion} el usuario {usuario.NombreCompleto} ({usuario.Rol.Nombre})");

            return ResultadoOperacion.Ok($"Usuario {usuario.NombreCompleto} {(usuario.Activo ? "activado" : "desactivado")}.");
        }

        /// <summary>
        /// Roles que se pueden asignar a un usuario nuevo. El dueño / vendedor no aparece:
        /// por decisión del dueño, él es el único administrador del sistema.
        /// </summary>
        public List<Rol> Roles() => _roles.ObtenerTodos()
            .Where(r => r.Tipo != TipoRol.DuenoVendedor)
            .OrderBy(r => r.Tipo)
            .ToList();

        public ResultadoOperacion<Usuario> Crear(NuevoUsuarioViewModel modelo, Usuario actor)
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
                ContrasenaHash = Seguridad.Hash(modelo.Contrasena),
                RolId = rol.Id,
                Rol = rol,
                Activo = modelo.Activo
            });
            rol.Usuarios.Add(usuario);

            _bitacora.Registrar(actor, null, TipoMovimiento.Usuario,
                $"Creó el usuario {usuario.NombreCompleto} ({rol.Nombre}) · {usuario.Correo} · {(usuario.Activo ? "Activo" : "Inactivo")}");

            return ResultadoOperacion<Usuario>.Ok(usuario, $"Usuario {usuario.NombreCompleto} creado. Inicia sesión con {usuario.Correo}.");
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

    /// <summary>
    /// Mantiene el usuario de la sesión. Guarda solo el Id en la sesión y lo busca en el repositorio,
    /// así un usuario desactivado pierde el acceso en su siguiente petición.
    /// </summary>
    public class UsuarioActualService : IUsuarioActualService
    {
        public const string ClaveSesion = "UsuarioId";

        private readonly IHttpContextAccessor _http;
        private readonly IUsuarioRepository _usuarios;
        private Usuario? _cache;
        private bool _cargado;

        public UsuarioActualService(IHttpContextAccessor http, IUsuarioRepository usuarios)
        {
            _http = http;
            _usuarios = usuarios;
        }

        public Usuario? Usuario
        {
            get
            {
                if (_cargado) return _cache;
                _cargado = true;

                var id = _http.HttpContext?.Session.GetInt32(ClaveSesion);
                var usuario = id.HasValue ? _usuarios.ObtenerPorId(id.Value) : null;
                _cache = usuario is { Activo: true } ? usuario : null;
                return _cache;
            }
        }

        public bool Autenticado => Usuario != null;

        public TipoRol? Rol => Usuario?.Rol.Tipo;

        public void IniciarSesion(Usuario usuario)
        {
            _http.HttpContext?.Session.SetInt32(ClaveSesion, usuario.Id);
            _cache = usuario;
            _cargado = true;
        }

        public void CerrarSesion()
        {
            _http.HttpContext?.Session.Clear();
            _cache = null;
            _cargado = true;
        }
    }
}
