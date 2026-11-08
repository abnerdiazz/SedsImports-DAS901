using Microsoft.AspNetCore.Mvc;
using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Web.Filters;

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
            Movimientos = _bitacora.Listar(busqueda),
            // Vehículos eliminados lógicamente: sus filas se marcan como "solo historial".
            VehiculosEliminados = _vehiculos.IdsEliminados()
        });

        // GET /Bitacora/Trazabilidad/5?origen=ventas — el botón "Volver" regresa a la pantalla de origen
        [HttpGet]
        public IActionResult Trazabilidad(int id, string? origen = null)
        {
            // Un vehículo eliminado ya no tiene ficha, pero su historial se sigue pudiendo consultar.
            var vehiculo = _vehiculos.ObtenerParaHistorial(id);
            if (vehiculo == null) return NotFound();

            return View(new TrazabilidadViewModel
            {
                VehiculoId = id,
                Vehiculo = vehiculo.NombreCompleto,
                Eliminado = vehiculo.Eliminado,
                FechaEliminacion = vehiculo.FechaEliminacion,
                EliminadoPor = vehiculo.EliminadoPorNombre,
                Origen = origen,
                Movimientos = _bitacora.PorVehiculo(id)
            });
        }
    }
}
