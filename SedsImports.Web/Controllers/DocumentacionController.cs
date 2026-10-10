using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Módulo de documentación (asistente administrativa; el dueño también puede actualizarla).</summary>
    [PerfilRequerido(TipoRol.AsistenteAdministrativa, TipoRol.DuenoVendedor)]
    public class DocumentacionController : BaseController
    {
        private readonly IDocumentacionService _documentacion;
        private readonly IVehiculoService _vehiculos;

        public DocumentacionController(IDocumentacionService documentacion, IVehiculoService vehiculos)
        {
            _documentacion = documentacion;
            _vehiculos = vehiculos;
        }

        [HttpGet]
        public IActionResult Index(string? busqueda, EstadoDocumentacion? filtro) =>
            View(_documentacion.Listar(busqueda, filtro));

        // GET /Documentacion/Editar/5 — versión en página completa del modal (sin JavaScript o con errores)
        [HttpGet]
        public IActionResult Editar(int id, string? origen = null)
        {
            if (_vehiculos.Obtener(id) == null) return NotFound();
            var modelo = _documentacion.ArmarFormulario(id);
            modelo.Origen = origen;
            return View(modelo);
        }

        [HttpPost]
        public IActionResult Editar(DocumentacionViewModel modelo)
        {
            var vehiculo = _vehiculos.Obtener(modelo.VehiculoId);
            if (vehiculo == null) return NotFound();
            modelo.Vehiculo = vehiculo.NombreCompleto;
            modelo.EstadoOperativo = vehiculo.Estado;

            if (!ModelState.IsValid) return View(modelo);

            var resultado = _documentacion.Actualizar(modelo, UsuarioActual);
            if (!resultado.Exitoso)
            {
                AgregarErrores(resultado);
                return View(modelo);
            }

            MostrarExito(resultado.Mensaje);
            return modelo.Origen == "ficha"
                ? VolverAFicha(modelo.VehiculoId)
                : RedirectToAction(nameof(Index));
        }
    }
}
