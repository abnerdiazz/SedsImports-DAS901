using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>
    /// Acciones del mecánico sobre un vehículo: iniciar revisión, anotar, subir fotos,
    /// finalizar (queda pendiente de autorización) y regresar a revisión.
    /// </summary>
    [PerfilRequerido(TipoRol.Mecanico)]
    public class RevisionesController : BaseController
    {
        private readonly IRevisionService _revisiones;
        private readonly IVehiculoService _vehiculos;

        public RevisionesController(IRevisionService revisiones, IVehiculoService vehiculos)
        {
            _revisiones = revisiones;
            _vehiculos = vehiculos;
        }

        [HttpPost]
        public IActionResult Iniciar(int vehiculoId)
        {
            MostrarResultado(_revisiones.Iniciar(vehiculoId, UsuarioActual));
            return VolverAFicha(vehiculoId);
        }

        [HttpPost]
        public IActionResult AgregarNota(NotaRevisionViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                MostrarError(PrimerError());
                return VolverAFicha(modelo.VehiculoId, "notas");
            }

            MostrarResultado(_revisiones.AgregarNota(modelo, UsuarioActual));
            return VolverAFicha(modelo.VehiculoId, "notas");
        }

        [HttpPost]
        public async Task<IActionResult> SubirFotos(SubirFotosViewModel modelo)
        {
            MostrarResultado(await _revisiones.SubirFotosAsync(modelo, UsuarioActual));
            return VolverAFicha(modelo.VehiculoId, "fotos");
        }

        // GET /Revisiones/Editar/5 — formulario para finalizar la revisión
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NotFound();

            if (vehiculo.Estado != EstadoVehiculo.EnRevision)
            {
                MostrarError("Solo se puede finalizar un vehículo que está en revisión.");
                return VolverAFicha(id);
            }

            return View(Completar(new FinalizarRevisionViewModel { VehiculoId = id }, vehiculo));
        }

        [HttpPost]
        public async Task<IActionResult> Editar(FinalizarRevisionViewModel modelo)
        {
            var vehiculo = _vehiculos.Obtener(modelo.VehiculoId);
            if (vehiculo == null) return NotFound();

            if (!ModelState.IsValid) return View(Completar(modelo, vehiculo));

            var resultado = await _revisiones.FinalizarAsync(modelo, UsuarioActual);
            if (!resultado.Exitoso)
            {
                AgregarErrores(resultado);
                return View(Completar(modelo, vehiculo));
            }

            MostrarExito(resultado.Mensaje);
            return VolverAFicha(modelo.VehiculoId);
        }

        // El dueño también puede devolver un vehículo a revisión (por ejemplo, al no autorizarlo).
        [HttpPost]
        [PerfilRequerido(TipoRol.Mecanico, TipoRol.DuenoVendedor)]
        public IActionResult Regresar(RegresarRevisionViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                MostrarError(PrimerError());
                return VolverAFicha(modelo.VehiculoId);
            }

            MostrarResultado(_revisiones.RegresarARevision(modelo, UsuarioActual));
            return VolverAFicha(modelo.VehiculoId);
        }

        private FinalizarRevisionViewModel Completar(FinalizarRevisionViewModel modelo, Vehiculo vehiculo)
        {
            var revision = _revisiones.ObtenerPorVehiculo(vehiculo.Id);
            modelo.Vehiculo = vehiculo.NombreCompleto;
            modelo.Vin = vehiculo.Vin;
            modelo.NotasPrevias = revision?.Notas.OrderByDescending(n => n.Fecha).ToList() ?? new();
            modelo.FotosFinales = vehiculo.Fotografias.Count(f => f.Tipo == TipoFoto.Final);
            return modelo;
        }
    }
}
