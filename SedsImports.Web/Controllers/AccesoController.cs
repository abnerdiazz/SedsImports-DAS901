using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Web.Servicios.Api;

namespace SedsImports.Web.Controllers
{
    /// <summary>
    /// Inicio y cierre de sesión. Desde la Fase 4 las credenciales se validan en la API REST
    /// (POST /api/auth/login, con ASP.NET Core Identity); la API devuelve un token JWT que se guarda
    /// cifrado en la cookie de autenticación para usarlo en las llamadas posteriores a la API.
    /// </summary>
    public class AccesoController : BaseController
    {
        private readonly ISedsApiCliente _api;
        private readonly IUsuarioActualService _sesion;
        private readonly IUsuarioService _usuarios;

        public AccesoController(ISedsApiCliente api, IUsuarioActualService sesion, IUsuarioService usuarios)
        {
            _api = api;
            _sesion = sesion;
            _usuarios = usuarios;
        }

        // GET /Acceso  (también es la página de inicio de la aplicación)
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (_sesion.Autenticado) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST /Acceso — el token antifalsificación (CSRF) se valida con el filtro global.
        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Index(LoginViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var respuesta = await _api.IniciarSesionAsync(modelo.Correo, modelo.Contrasena);
            var usuario = respuesta.Exitosa ? _usuarios.Obtener(respuesta.Dato!.Usuario.Id) : null;
            if (!respuesta.Exitosa || usuario == null)
            {
                ModelState.AddModelError(string.Empty, respuesta.Error ?? "Usuario o contraseña incorrectos.");
                modelo.Contrasena = string.Empty;
                return View(modelo);
            }

            var token = respuesta.Dato!;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.NombreCompleto),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol.Tipo.ToString())
            };

            var propiedades = new AuthenticationProperties
            {
                // La sesión de la web dura lo mismo que el token de la API.
                ExpiresUtc = DateTime.SpecifyKind(token.Expira, DateTimeKind.Utc),
                IsPersistent = false,
                AllowRefresh = false
            };
            propiedades.StoreTokens(new[] { new AuthenticationToken { Name = ManejadorTokenApi.NombreToken, Value = token.Token } });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), propiedades);
            _sesion.IniciarSesion(usuario);

            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return LocalRedirect(modelo.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _sesion.CerrarSesion();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Denegado() => View();
    }
}
