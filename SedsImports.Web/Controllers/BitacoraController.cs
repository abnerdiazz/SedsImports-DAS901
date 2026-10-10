using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Bitácora general de cambios y trazabilidad de cada vehículo (dueño / vendedor).</summary>
    [PerfilRequerido(TipoRol.DuenoVendedor)]
    public class BitacoraController : BaseController
    {
        private readonly IBitacoraService _bitacora;
        private readonly IVehiculoService _vehiculos;

        public BitacoraController(IBitacoraService bitacora, IVehiculoService vehiculos)
        {
            _bitacora = bitacora;
            _vehiculos = vehiculos;
        }

        [HttpGet]
        public IActionResult Index(string? busqueda) => View(new BitacoraViewModel
        {
            Busqueda = busqueda,
            Movimientos = _bitacora.Listar(busqueda)
        });

        // GET /Bitacora/Trazabilidad/5?origen=ventas — el botón "Volver" regresa a la pantalla de origen
        [HttpGet]
        public IActionResult Trazabilidad(int id, string? origen = null)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NotFound();

            return View(new TrazabilidadViewModel
            {
                VehiculoId = id,
                Vehiculo = vehiculo.NombreCompleto,
                Origen = origen,
                Movimientos = _bitacora.PorVehiculo(id)
            });
        }
    }
}
