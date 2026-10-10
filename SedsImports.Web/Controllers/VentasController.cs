using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Ventas registradas, registro de venta y reporte de ventas (dueño / vendedor).</summary>
    [PerfilRequerido(TipoRol.DuenoVendedor)]
    public class VentasController : BaseController
    {
        private readonly IVentaService _ventas;
        private readonly IVehiculoService _vehiculos;
        private readonly IApartadoService _apartados;
        private readonly IClienteService _clientes;

        public VentasController(IVentaService ventas, IVehiculoService vehiculos, IApartadoService apartados, IClienteService clientes)
        {
            _ventas = ventas;
            _vehiculos = vehiculos;
            _apartados = apartados;
            _clientes = clientes;
        }

        [HttpGet]
        public IActionResult Index() => View(_ventas.Listar());

        // GET /Ventas/Crear?vehiculoId=5 — versión en página completa del modal
        [HttpGet]
        public IActionResult Crear(int vehiculoId)
        {
            var vehiculo = _vehiculos.Obtener(vehiculoId);
            if (vehiculo == null) return NotFound();

            if (vehiculo.Estado is not (EstadoVehiculo.Apartado or EstadoVehiculo.Disponible))
            {
                MostrarError("Solo se puede vender un vehículo disponible o apartado.");
                return VolverAFicha(vehiculoId);
            }

            var apartado = _apartados.ObtenerActivo(vehiculoId);
            var cliente = apartado != null ? _clientes.Obtener(apartado.ClienteId) : null;

            return View(new VentaViewModel
            {
                VehiculoId = vehiculoId,
                Vehiculo = vehiculo.NombreCompleto,
                VieneDeApartado = apartado != null,
                AnticipoPrevio = apartado?.Anticipo,
                NombreCliente = cliente?.Nombre ?? string.Empty,
                Telefono = cliente?.Telefono,
                FechaVenta = DateTime.Today
            });
        }

        [HttpPost]
        public IActionResult Crear(VentaViewModel modelo)
        {
            var vehiculo = _vehiculos.Obtener(modelo.VehiculoId);
            if (vehiculo == null) return NotFound();
            modelo.Vehiculo = vehiculo.NombreCompleto;
            modelo.VieneDeApartado = _apartados.ObtenerActivo(vehiculo.Id) != null;

            if (!ModelState.IsValid) return View(modelo);

            var resultado = _ventas.Registrar(modelo, UsuarioActual);
            if (!resultado.Exitoso)
            {
                AgregarErrores(resultado);
                return View(modelo);
            }

            MostrarExito(resultado.Mensaje);
            return VolverAFicha(modelo.VehiculoId);
        }

        // GET /Ventas/Reporte?anio=2026&mes=9&dia=&busqueda=corolla
        [HttpGet]
        public IActionResult Reporte(ReporteViewModel filtros)
        {
            var anios = _ventas.AniosConVentas();
            if (!anios.Contains(filtros.Anio)) filtros.Anio = DateTime.Today.Year;

            filtros.AniosDisponibles = anios;
            filtros.Resultado = _ventas.Reporte(filtros.Anio, filtros.Mes, filtros.Dia, filtros.Busqueda);
            return View(filtros);
        }
    }
}
