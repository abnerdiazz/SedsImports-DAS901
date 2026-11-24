using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;
using SedsImports.Domain.Enums;

namespace SedsImports.Api.Controllers
{
    /// <summary>Expediente de documentos de cada vehículo (asistente administrativa y dueño).</summary>
    [Authorize(Policy = Politicas.Documentacion)]
    public class DocumentacionController : ApiControllerBase
    {
        private readonly IDocumentacionService _documentacion;
        private readonly IVehiculoService _vehiculos;

        public DocumentacionController(IDocumentacionService documentacion, IVehiculoService vehiculos)
        {
            _documentacion = documentacion;
            _vehiculos = vehiculos;
        }

        /// <summary>Lista la documentación de los vehículos con búsqueda y filtro por estado.</summary>
        /// <param name="busqueda">Texto a buscar en el vehículo o VIN.</param>
        /// <param name="estado">Pendiente o Completa.</param>
        [HttpGet("api/documentaciones")]
        [ProducesResponseType(typeof(List<DocumentacionDto>), StatusCodes.Status200OK)]
        public ActionResult<List<DocumentacionDto>> Listar([FromQuery] string? busqueda, [FromQuery] EstadoDocumentacion? estado)
        {
            var listado = _documentacion.Listar(busqueda, estado);
            return Ok(listado.Filas.Select(f => f.Documentacion.ADto(f.Vehiculo.Nombre)).ToList());
        }

        /// <summary>Documentación de un vehículo.</summary>
        /// <response code="200">Checklist del vehículo.</response>
        /// <response code="404">El vehículo no existe.</response>
        [HttpGet("api/vehiculos/{vehiculoId:int}/documentacion")]
        [ProducesResponseType(typeof(DocumentacionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<DocumentacionDto> Obtener(int vehiculoId)
        {
            var vehiculo = _vehiculos.Obtener(vehiculoId);
            if (vehiculo == null) return NoEncontrado("El vehículo no existe.");

            return Ok(_documentacion.ObtenerPorVehiculo(vehiculoId).ADto($"{vehiculo.Marca} {vehiculo.Modelo} {vehiculo.Anio}"));
        }

        /// <summary>Actualiza el checklist de documentos y su estado.</summary>
        /// <remarks>Para marcarla como Completa deben estar los cuatro documentos principales.</remarks>
        /// <response code="204">Documentación actualizada.</response>
        /// <response code="400">Datos no válidos o documentos incompletos para el estado indicado.</response>
        /// <response code="404">El vehículo no existe.</response>
        [HttpPut("api/vehiculos/{vehiculoId:int}/documentacion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Actualizar(int vehiculoId, [FromBody] ActualizarDocumentacionDto dto)
        {
            // Reglas del modelo (por ejemplo, "Completa" exige los cuatro documentos principales).
            var modelo = dto.AViewModel(vehiculoId);
            foreach (var error in modelo.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(modelo)))
                foreach (var campo in error.MemberNames.DefaultIfEmpty(string.Empty))
                    ModelState.AddModelError(campo, error.ErrorMessage ?? "Dato no válido.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            return Responder(_documentacion.Actualizar(modelo, Actor), NoContent);
        }
    }
}
