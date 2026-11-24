using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;

namespace SedsImports.Api.Controllers
{
    /// <summary>Ventas de vehículos (solo el dueño / vendedor).</summary>
    [Authorize(Policy = Politicas.SoloDueno)]
    public class VentasController : ApiControllerBase
    {
        private readonly IVentaService _ventas;

        public VentasController(IVentaService ventas) => _ventas = ventas;

        /// <summary>Ventas registradas, la más reciente primero.</summary>
        [HttpGet("api/ventas")]
        [ProducesResponseType(typeof(List<VentaDetalleDto>), StatusCodes.Status200OK)]
        public ActionResult<List<VentaDetalleDto>> Listar() => Ok(_ventas.Listar());

        /// <summary>Venta de un vehículo.</summary>
        /// <response code="200">Datos de la venta.</response>
        /// <response code="404">El vehículo no se ha vendido.</response>
        [HttpGet("api/vehiculos/{vehiculoId:int}/venta", Name = nameof(ObtenerVenta))]
        [ProducesResponseType(typeof(VentaDetalleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VentaDetalleDto> ObtenerVenta(int vehiculoId)
        {
            var venta = _ventas.ObtenerPorVehiculo(vehiculoId);
            if (venta == null) return NoEncontrado("El vehículo no se ha vendido.");

            return Ok(new VentaDetalleDto
            {
                VentaId = venta.Id,
                VehiculoId = venta.VehiculoId,
                Vehiculo = venta.Vehiculo != null ? $"{venta.Vehiculo.Marca} {venta.Vehiculo.Modelo} {venta.Vehiculo.Anio}" : string.Empty,
                Cliente = venta.Cliente?.Nombre ?? string.Empty,
                Telefono = venta.Cliente?.Telefono,
                Fecha = venta.FechaVenta,
                Monto = venta.Monto
            });
        }

        /// <summary>Registra la venta de un vehículo disponible o apartado; el vehículo pasa a "Vendido".</summary>
        /// <response code="201">Venta registrada.</response>
        /// <response code="400">Datos no válidos.</response>
        /// <response code="409">El vehículo no está disponible ni apartado, o ya se vendió.</response>
        [HttpPost("api/ventas")]
        [ProducesResponseType(typeof(VentaDetalleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Registrar([FromBody] CrearVentaDto dto)
        {
            var resultado = _ventas.Registrar(dto.AViewModel(), Actor);
            return Responder(resultado, () =>
            {
                var creada = ObtenerVenta(dto.VehiculoId).Result as OkObjectResult;
                return CreatedAtRoute(nameof(ObtenerVenta), new { vehiculoId = dto.VehiculoId }, creada?.Value);
            });
        }
    }
}
