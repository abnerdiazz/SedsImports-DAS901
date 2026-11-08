using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Web.Servicios
{
    /// <summary>
    /// Mantiene el usuario de la sesión. Guarda solo el Id en la sesión y lo busca en la base de datos
    /// en cada petición, así un usuario desactivado pierde el acceso en su siguiente petición.
    /// Vive en la capa web porque depende de la sesión HTTP.
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
