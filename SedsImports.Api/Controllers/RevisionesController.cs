using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;

namespace SedsImports.Api.Controllers
{
    /// <summary>
    /// Revisión y preparación del vehículo (mecánico): iniciar, anotar trabajos, finalizar y
    /// regresar a revisión. Es un recurso que depende del vehículo: /api/vehiculos/{id}/revision.
    /// </summary>
    [Route("api/vehiculos/{vehiculoId:int}/revision")]
    public class RevisionesController : ApiControllerBase
    {
        private readonly IRevisionService _revisiones;
        private readonly IVehiculoService _vehiculos;

        public RevisionesController(IRevisionService revisiones, IVehiculoService vehiculos)
        {
            _revisiones = revisiones;
            _vehiculos = vehiculos;
        }

        /// <summary>Revisión del vehículo con todas sus anotaciones (la más reciente primero).</summary>
        /// <response code="200">Revisión del vehículo.</response>
        /// <response code="404">El vehículo no existe o todavía no inicia su revisión.</response>
        [HttpGet]
        [ProducesResponseType(typeof(RevisionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RevisionDto> Obtener(int vehiculoId)
        {
            if (_vehiculos.Obtener(vehiculoId) == null) return NoEncontrado("El vehículo no existe.");

            var revision = _revisiones.ObtenerPorVehiculo(vehiculoId);
            return revision == null
                ? NoEncontrado("El vehículo todavía no tiene una revisión iniciada.")
                : Ok(revision.ADto());
        }

        /// <summary>Inicia la revisión de un vehículo recién llegado.</summary>
        /// <response code="201">Revisión iniciada; el vehículo pasa a "En revisión".</response>
        /// <response code="409">El vehículo no está recién llegado.</response>
        [HttpPost]
        [Authorize(Policy = Politicas.Taller)]
        [ProducesResponseType(typeof(RevisionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Iniciar(int vehiculoId)
        {
            var resultado = _revisiones.Iniciar(vehiculoId, Actor);
            return Responder(resultado, () =>
                CreatedAtAction(nameof(Obtener), new { vehiculoId }, _revisiones.ObtenerPorVehiculo(vehiculoId)!.ADto()));
        }

        /// <summary>Lista las anotaciones de la revisión.</summary>
        /// <response code="200">Anotaciones (la más reciente primero).</response>
        [HttpGet("notas")]
        [ProducesResponseType(typeof(List<NotaDto>), StatusCodes.Status200OK)]
        public ActionResult<List<NotaDto>> Notas(int vehiculoId)
        {
            var revision = _revisiones.ObtenerPorVehiculo(vehiculoId);
            return Ok(revision?.ADto().Notas ?? new List<NotaDto>());
        }

        /// <summary>Agrega una anotación de trabajo a la revisión en curso.</summary>
        /// <response code="201">Anotación registrada (se devuelve la anotación creada).</response>
        /// <response code="400">Texto vacío o demasiado largo.</response>
        /// <response code="409">El vehículo no está en revisión.</response>
        [HttpPost("notas")]
        [Authorize(Policy = Politicas.Taller)]
        [ProducesResponseType(typeof(NotaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult AgregarNota(int vehiculoId, [FromBody] CrearNotaDto dto)
        {
            var resultado = _revisiones.AgregarNota(dto.AViewModel(vehiculoId), Actor);
            return Responder(resultado, () =>
            {
                var nota = _revisiones.ObtenerPorVehiculo(vehiculoId)!.Notas
                    .Where(n => !n.EsDelSistema).OrderByDescending(n => n.Fecha).ThenByDescending(n => n.Id).First();
                return CreatedAtAction(nameof(Notas), new { vehiculoId }, nota.ADto());
            });
        }

        /// <summary>Finaliza la revisión con el resumen de trabajos; el vehículo queda pendiente de autorización.</summary>
        /// <response code="204">Revisión finalizada.</response>
        /// <response code="400">Falta el resumen.</response>
        /// <response code="409">El vehículo no está en revisión.</response>
        [HttpPut("finalizacion")]
        [Authorize(Policy = Politicas.Taller)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Finalizar(int vehiculoId, [FromBody] FinalizarRevisionDto dto) =>
            Responder(await _revisiones.FinalizarAsync(dto.AViewModel(vehiculoId), Actor), NoContent);

        /// <summary>Regresa el vehículo a revisión indicando el motivo.</summary>
        /// <response code="204">El vehículo vuelve a "En revisión".</response>
        /// <response code="409">El vehículo está apartado o vendido.</response>
        [HttpPost("regreso")]
        [Authorize(Policy = Politicas.TallerODueno)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Regresar(int vehiculoId, [FromBody] RegresarRevisionDto dto) =>
            Responder(_revisiones.RegresarARevision(dto.AViewModel(vehiculoId), Actor), NoContent);
    }
}
