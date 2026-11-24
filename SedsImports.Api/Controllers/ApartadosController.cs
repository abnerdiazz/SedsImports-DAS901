using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;

namespace SedsImports.Api.Controllers
{
    /// <summary>Apartados de vehículos disponibles con anticipo del cliente (solo el dueño / vendedor).</summary>
    [Authorize(Policy = Politicas.SoloDueno)]
    public class ApartadosController : ApiControllerBase
    {
        private readonly IApartadoService _apartados;

        public ApartadosController(IApartadoService apartados) => _apartados = apartados;

        /// <summary>Apartados activos con el vehículo, el cliente y el anticipo.</summary>
        [HttpGet("api/apartados")]
        [ProducesResponseType(typeof(List<ApartadoListadoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<ApartadoListadoDto>> Listar() => Ok(_apartados.ListarActivos());

        /// <summary>Apartado activo de un vehículo.</summary>
        /// <response code="200">Apartado activo.</response>
        /// <response code="404">El vehículo no tiene un apartado activo.</response>
        [HttpGet("api/vehiculos/{vehiculoId:int}/apartado", Name = nameof(ObtenerApartado))]
        [ProducesResponseType(typeof(ApartadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApartadoDto> ObtenerApartado(int vehiculoId)
        {
            var apartado = _apartados.ObtenerActivo(vehiculoId);
            return apartado == null ? NoEncontrado("El vehículo no tiene un apartado activo.") : Ok(apartado.ADto());
        }

        /// <summary>Registra el apartado de un vehículo disponible; el vehículo pasa a "Apartado".</summary>
        /// <response code="201">Apartado registrado.</response>
        /// <response code="400">Datos no válidos (anticipo, teléfono...).</response>
        /// <response code="409">El vehículo no está disponible.</response>
        [HttpPost("api/apartados")]
        [ProducesResponseType(typeof(ApartadoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Registrar([FromBody] CrearApartadoDto dto)
        {
            var resultado = _apartados.Registrar(dto.AViewModel(), Actor);
            return Responder(resultado, () =>
                CreatedAtRoute(nameof(ObtenerApartado), new { vehiculoId = dto.VehiculoId }, _apartados.ObtenerActivo(dto.VehiculoId)!.ADto()));
        }

        /// <summary>Cancela el apartado activo; el vehículo vuelve a "Disponible".</summary>
        /// <response code="204">Apartado cancelado.</response>
        /// <response code="409">El vehículo no tiene un apartado activo.</response>
        [HttpPost("api/vehiculos/{vehiculoId:int}/apartado/cancelacion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Cancelar(int vehiculoId, [FromBody] CancelarApartadoDto dto) =>
            Responder(_apartados.Cancelar(dto.AViewModel(vehiculoId), Actor), NoContent);
    }
}
