using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Administración de usuarios internos: alta, activar y desactivar. Solo el dueño.</summary>
    [PerfilRequerido(TipoRol.DuenoVendedor)]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarios;

        public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

        [HttpGet]
        public IActionResult Index() => View(ArmarModelo(new NuevoUsuarioViewModel()));

        // POST /Usuarios/Crear — formulario del modal "Agregar usuario"
        [HttpPost]
        public IActionResult Crear([Bind(Prefix = "NuevoUsuario")] NuevoUsuarioViewModel nuevo)
        {
            if (ModelState.IsValid)
            {
                var resultado = _usuarios.Crear(nuevo, UsuarioActual);
                if (resultado.Exitoso)
                {
                    MostrarExito(resultado.Mensaje);
                    return RedirectToAction(nameof(Index));
                }
                AgregarErrores(resultado, "NuevoUsuario");
            }

            // Con errores se vuelve a mostrar la lista con el modal abierto y los datos escritos
            // (las contraseñas no se devuelven).
            nuevo.Contrasena = nuevo.ConfirmarContrasena = string.Empty;
            nuevo.Correo = _usuarios.GenerarCorreo(nuevo.Nombre, nuevo.Apellido);
            var modelo = ArmarModelo(nuevo);
            modelo.AbrirModalNuevo = true;
            return View(nameof(Index), modelo);
        }

        // GET /Usuarios/CorreoSugerido?nombre=Lucía&apellido=Mena — vista previa del usuario en el modal
        [HttpGet]
        public IActionResult CorreoSugerido(string? nombre, string? apellido) =>
            Json(new { correo = _usuarios.GenerarCorreo(nombre, apellido) });

        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            MostrarResultado(_usuarios.CambiarEstado(id, UsuarioActual));
            return RedirectToAction(nameof(Index));
        }

        private UsuariosViewModel ArmarModelo(NuevoUsuarioViewModel nuevo) => new()
        {
            UsuarioActualId = UsuarioActual.Id,
            Usuarios = _usuarios.Listar(),
            Roles = _usuarios.Roles(),
            NuevoUsuario = nuevo
        };
    }
}
