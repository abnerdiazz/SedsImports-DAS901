using System.Security.Claims;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Web.Servicios
{
    /// <summary>
    /// Usuario que inició sesión en la aplicación web. Desde la Fase 4 se identifica con la cookie de
    /// autenticación de ASP.NET Core (claims), que se crea con los datos que devuelve la API al iniciar
    /// sesión. Se carga de la base de datos una sola vez por petición.
    /// </summary>
    public class UsuarioActualService : IUsuarioActualService
    {
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

                var principal = _http.HttpContext?.User;
                var valor = principal?.Identity?.IsAuthenticated == true ? principal.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                var usuario = int.TryParse(valor, out var id) ? _usuarios.ObtenerPorId(id) : null;

                // Si el dueño desactiva al usuario, pierde el acceso aunque su cookie siga vigente.
                _cache = usuario is { Activo: true } ? usuario : null;
                return _cache;
            }
        }

        public bool Autenticado => Usuario != null;

        public TipoRol? Rol => Usuario?.Rol.Tipo;

        /// <summary>La cookie la crea AccesoController; aquí solo se deja el usuario listo para la petición actual.</summary>
        public void IniciarSesion(Usuario usuario)
        {
            _cache = usuario;
            _cargado = true;
        }

        public void CerrarSesion()
        {
            _cache = null;
            _cargado = true;
        }
    }
}
