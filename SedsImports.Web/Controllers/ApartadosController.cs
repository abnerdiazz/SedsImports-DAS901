using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Vehículos disponibles para ofrecer y apartados con anticipo (dueño / vendedor).</summary>
    [PerfilRequerido(TipoRol.DuenoVendedor)]
    public class ApartadosController : BaseController
    {
        private readonly IApartadoService _apartados;
        private readonly IVehiculoService _vehiculos;

        public ApartadosController(IApartadoService apartados, IVehiculoService vehiculos)
        {
            _apartados = apartados;
            _vehiculos = vehiculos;
        }

        [HttpGet]
        public IActionResult Index() => View(new ApartadosIndexViewModel
        {
            Apartados = _apartados.ListarActivos(),
            PuedeGestionar = Permisos.PuedeGestionarComercial(RolActual)
        });

        // GET /Apartados/Disponibles — tarjetas de vehículos listos para ofrecer
        [HttpGet]
        public IActionResult Disponibles() => View(new DisponiblesViewModel
        {
            Vehiculos = _vehiculos.Listar(estado: EstadoVehiculo.Disponible),
            NuevoApartado = new ApartadoViewModel { Origen = "disponibles" },
            PuedeApartar = Permisos.PuedeGestionarComercial(RolActual)
        });

        // GET /Apartados/Crear?vehiculoId=5 — versión en página completa del modal
        [HttpGet]
        public IActionResult Crear(int vehiculoId, string? origen = null)
        {
            var vehiculo = _vehiculos.Obtener(vehiculoId);
            if (vehiculo == null) return NotFound();

            if (vehiculo.Estado != EstadoVehiculo.Disponible)
            {
                MostrarError($"{vehiculo.NombreCompleto} no está disponible para apartar.");
                return RedirectToAction(nameof(Disponibles));
            }

            return View(new ApartadoViewModel { VehiculoId = vehiculoId, Vehiculo = vehiculo.NombreCompleto, Origen = origen });
        }

        [HttpPost]
        public IActionResult Crear(ApartadoViewModel modelo)
        {
            var vehiculo = _vehiculos.Obtener(modelo.VehiculoId);
            if (vehiculo == null) return NotFound();
            modelo.Vehiculo = vehiculo.NombreCompleto;

            if (!ModelState.IsValid) return View(modelo);

            var resultado = _apartados.Registrar(modelo, UsuarioActual);
            if (!resultado.Exitoso)
            {
                AgregarErrores(resultado);
                return View(modelo);
            }

            MostrarExito(resultado.Mensaje);
            return modelo.Origen == "ficha" ? VolverAFicha(modelo.VehiculoId) : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Cancelar(CancelarApartadoViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                MostrarError(PrimerError());
                return VolverAFicha(modelo.VehiculoId);
            }

            MostrarResultado(_apartados.Cancelar(modelo, UsuarioActual));
            return VolverAFicha(modelo.VehiculoId);
        }
    }
}
