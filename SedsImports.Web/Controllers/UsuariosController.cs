using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Administración de usuarios internos (activar / desactivar). Solo el dueño.</summary>
    [PerfilRequerido(TipoRol.DuenoVendedor)]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarios;

        public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

        [HttpGet]
        public IActionResult Index() => View(new UsuariosViewModel
        {
            UsuarioActualId = UsuarioActual.Id,
            Usuarios = _usuarios.Listar()
        });

        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            MostrarResultado(_usuarios.CambiarEstado(id, UsuarioActual));
            return RedirectToAction(nameof(Index));
        }
    }
}
