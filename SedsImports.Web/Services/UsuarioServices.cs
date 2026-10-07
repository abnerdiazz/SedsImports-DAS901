using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;

namespace SedsImports.Web.Services
{
    /// <summary>Acceso al sistema y administración de usuarios internos (RF-01).</summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarios;
        private readonly IBitacoraService _bitacora;

        public UsuarioService(IUsuarioRepository usuarios, IBitacoraService bitacora)
        {
            _usuarios = usuarios;
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
