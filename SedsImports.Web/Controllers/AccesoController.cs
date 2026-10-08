using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Inicio y cierre de sesión, y página de acceso denegado.</summary>
    public class AccesoController : BaseController
    {
        private readonly IUsuarioService _usuarios;
        private readonly IUsuarioActualService _sesion;

        public AccesoController(IUsuarioService usuarios, IUsuarioActualService sesion)
        {
            _usuarios = usuarios;
            _sesion = sesion;
        }

        // GET /Acceso  (también es la página de inicio de la aplicación)
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (_sesion.Autenticado) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST /Acceso  — Model Binding: los campos del formulario llenan LoginViewModel.
        [HttpPost]
        public IActionResult Index(LoginViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var resultado = _usuarios.Autenticar(modelo.Correo, modelo.Contrasena);
            if (!resultado.Exitoso || resultado.Dato == null)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                modelo.Contrasena = string.Empty;
                return View(modelo);
            }

            _sesion.IniciarSesion(resultado.Dato);

            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return LocalRedirect(modelo.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Salir()
        {
            _sesion.CerrarSesion();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Denegado() => View();
    }
}
