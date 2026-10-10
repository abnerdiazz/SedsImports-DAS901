using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;

namespace SedsImports.Web.Filters
{
    /// <summary>
    /// Filtro de acceso por perfil. Se coloca sobre un controlador o una acción:
    ///   [PerfilRequerido]                                → solo exige haber iniciado sesión.
    ///   [PerfilRequerido(TipoRol.DuenoVendedor)]         → además exige uno de los roles indicados.
    /// Sin sesión redirige al inicio de sesión; con otro rol, a "Acceso denegado".
    /// (La autenticación y autorización formales se implementan en la Fase 4.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PerfilRequeridoAttribute : ActionFilterAttribute
    {
        private readonly TipoRol[] _roles;

        public PerfilRequeridoAttribute(params TipoRol[] roles)
        {
            _roles = roles;
            Order = 0;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Si la acción tiene su propio [PerfilRequerido], ese manda sobre el del controlador.
            var filtroAccion = context.ActionDescriptor.FilterDescriptors
                .Where(f => f.Scope == FilterScope.Action)
                .Select(f => f.Filter)
                .OfType<PerfilRequeridoAttribute>()
                .FirstOrDefault();
            if (filtroAccion != null && !ReferenceEquals(filtroAccion, this)) return;

            var sesion = context.HttpContext.RequestServices.GetRequiredService<IUsuarioActualService>();
            var usuario = sesion.Usuario;

            if (usuario == null)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Index", "Acceso", new { returnUrl });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(usuario.Rol.Tipo))
            {
                context.Result = new RedirectToActionResult("Denegado", "Acceso", null);
            }
        }
    }
}
