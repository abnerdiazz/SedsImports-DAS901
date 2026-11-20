using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Api.Seguridad
{
    /// <summary>
    /// Usuario que hace la petición a la API. Se obtiene del token JWT (claim "uid") y se carga
    /// de la base de datos una sola vez por petición. Los servicios lo reciben como "actor"
    /// para validar permisos y registrar la bitácora.
    /// </summary>
    public class UsuarioActualApi : IUsuarioActualService
    {
        private readonly IHttpContextAccessor _http;
        private readonly IUsuarioRepository _usuarios;
        private Usuario? _cache;
        private bool _cargado;

        public UsuarioActualApi(IHttpContextAccessor http, IUsuarioRepository usuarios)
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

                var valor = _http.HttpContext?.User.FindFirst(ClaimsSeds.UsuarioId)?.Value;
                var usuario = int.TryParse(valor, out var id) ? _usuarios.ObtenerPorId(id) : null;

                // Un usuario desactivado deja de tener acceso aunque su token todavía no haya vencido.
                _cache = usuario is { Activo: true } ? usuario : null;
                return _cache;
            }
        }

        public bool Autenticado => Usuario != null;

        public TipoRol? Rol => Usuario?.Rol.Tipo;

        // En la API no hay sesión: cada petición trae su token.
        public void IniciarSesion(Usuario usuario) => throw new NotSupportedException("La API no usa sesión; usa el token JWT.");

        public void CerrarSesion() => throw new NotSupportedException("La API no usa sesión; el token vence solo.");
    }
}
