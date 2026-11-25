using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;

namespace SedsImports.Api.Controllers
{
    /// <summary>Bitácora de auditoría: quién hizo qué, cuándo y sobre qué vehículo (solo el dueño).</summary>
    [Authorize(Policy = Politicas.SoloDueno)]
    public class BitacoraController : ApiControllerBase
    {
        private readonly IBitacoraService _bitacora;

        public BitacoraController(IBitacoraService bitacora) => _bitacora = bitacora;

        /// <summary>Movimientos de la bitácora, paginados y con búsqueda.</summary>
        /// <param name="busqueda">Usuario, vehículo o acción.</param>
        /// <param name="pagina">Número de página (desde 1).</param>
        /// <param name="tamano">Elementos por página (1 a 100).</param>
        [HttpGet("api/bitacora")]
        [ProducesResponseType(typeof(PaginaDto<MovimientoDto>), StatusCodes.Status200OK)]
        public ActionResult<PaginaDto<MovimientoDto>> Listar([FromQuery] string? busqueda, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        {
            pagina = Math.Max(1, pagina);
            tamano = Math.Clamp(tamano, 1, 100);

            var todos = _bitacora.Listar(busqueda);
            return Ok(new PaginaDto<MovimientoDto>
            {
                Pagina = pagina,
                TamanoPagina = tamano,
                TotalElementos = todos.Count,
                Elementos = todos.Skip((pagina - 1) * tamano).Take(tamano).Select(m => m.ADto()).ToList()
            });
        }

        /// <summary>Trazabilidad completa de un vehículo.</summary>
        [HttpGet("api/vehiculos/{vehiculoId:int}/bitacora")]
        [ProducesResponseType(typeof(List<MovimientoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<MovimientoDto>> PorVehiculo(int vehiculoId) =>
            Ok(_bitacora.PorVehiculo(vehiculoId).Select(m => m.ADto()).ToList());
    }
}
