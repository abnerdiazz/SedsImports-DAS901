using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;
using SedsImports.Domain.Enums;

namespace SedsImports.Api.Controllers
{
    /// <summary>
    /// Vehículos: consulta, registro, edición, autorización de disponibilidad y eliminación lógica.
    /// Usa los mismos servicios que la aplicación web, así las reglas del negocio son las mismas.
    /// </summary>
    [Route("api/vehiculos")]
    public class VehiculosController : ApiControllerBase
    {
        private readonly IVehiculoService _vehiculos;

        public VehiculosController(IVehiculoService vehiculos) => _vehiculos = vehiculos;

        /// <summary>Lista los vehículos que puede ver el usuario, con búsqueda y filtro por estado.</summary>
        /// <param name="busqueda">Texto a buscar en marca, modelo, año o VIN.</param>
        /// <param name="estado">Estado del vehículo (RecienLlegado, EnRevision, RevisionFinalizada, Disponible, Apartado, Vendido).</param>
        /// <response code="200">Lista de vehículos (puede estar vacía).</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<VehiculoListadoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<VehiculoListadoDto>> Listar([FromQuery] string? busqueda, [FromQuery] EstadoVehiculo? estado)
        {
            var visibles = Permisos.EstadosVisibles(RolActor);
            return Ok(_vehiculos.Listar(busqueda, estado, visibles));
        }

        /// <summary>Total de vehículos y cantidad por estado (tarjetas del panel de inicio).</summary>
        /// <response code="200">Conteos por estado.</response>
        [HttpGet("conteos")]
        [ProducesResponseType(typeof(ConteosVehiculosDto), StatusCodes.Status200OK)]
        public ActionResult<ConteosVehiculosDto> Conteos() =>
            Ok(_vehiculos.Conteos(Permisos.EstadosVisibles(RolActor)).ADto());

        /// <summary>Ficha de un vehículo.</summary>
        /// <param name="id">Id del vehículo.</param>
        /// <response code="200">Datos del vehículo.</response>
        /// <response code="403">El perfil no tiene acceso a vehículos en ese estado.</response>
        /// <response code="404">El vehículo no existe o fue eliminado.</response>
        [HttpGet("{id:int}", Name = nameof(ObtenerVehiculo))]
        [ProducesResponseType(typeof(VehiculoDetalleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VehiculoDetalleDto> ObtenerVehiculo(int id)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NoEncontrado("El vehículo no existe o ya fue eliminado.");

            if (!Permisos.EstadosVisibles(RolActor).Contains(vehiculo.Estado))
                return Problem("Tu perfil no tiene acceso a este vehículo.", statusCode: StatusCodes.Status403Forbidden, title: "Acceso denegado");

            return Ok(vehiculo.ADetalleDto());
        }

        /// <summary>Registra un vehículo recién llegado al patio.</summary>
        /// <remarks>
        /// El VIN no se puede repetir. El vehículo queda en estado "Recién llegado" y se crea su
        /// expediente de documentación vacío.
        /// </remarks>
        /// <response code="201">Vehículo registrado; el encabezado Location indica su dirección.</response>
        /// <response code="400">Datos no válidos (detalle por campo).</response>
        /// <response code="403">El perfil no puede registrar vehículos.</response>
        [HttpPost]
        [Authorize(Policy = Politicas.GestionPatio)]
        [ProducesResponseType(typeof(VehiculoDetalleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Registrar([FromBody] CrearVehiculoDto dto)
        {
            var resultado = await _vehiculos.RegistrarAsync(dto.AFormulario(), Actor);
            return Responder(resultado, () =>
                CreatedAtRoute(nameof(ObtenerVehiculo), new { id = resultado.Dato!.Id }, resultado.Dato!.ADetalleDto()));
        }

        /// <summary>Edita los datos generales de un vehículo.</summary>
        /// <param name="id">Id del vehículo.</param>
        /// <param name="dto">Datos nuevos.</param>
        /// <response code="204">Cambios guardados.</response>
        /// <response code="400">Datos no válidos.</response>
        /// <response code="404">El vehículo no existe.</response>
        /// <response code="409">El vehículo ya se vendió y no se puede editar.</response>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Politicas.GestionPatio)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Editar(int id, [FromBody] ActualizarVehiculoDto dto)
        {
            var resultado = await _vehiculos.EditarAsync(dto.AFormulario(id), Actor);
            return Responder(resultado, NoContent);
        }

        /// <summary>Autoriza que el vehículo quede disponible para la venta (después de la revisión).</summary>
        /// <param name="id">Id del vehículo.</param>
        /// <response code="204">Vehículo disponible para la venta.</response>
        /// <response code="409">La revisión todavía no está finalizada.</response>
        [HttpPost("{id:int}/autorizacion")]
        [Authorize(Policy = Politicas.SoloDueno)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Autorizar(int id) =>
            Responder(_vehiculos.AutorizarDisponibilidad(id, Actor), NoContent);

        /// <summary>Elimina un vehículo (eliminación lógica: se conserva en la bitácora).</summary>
        /// <param name="id">Id del vehículo.</param>
        /// <response code="204">Vehículo eliminado.</response>
        /// <response code="404">El vehículo no existe.</response>
        /// <response code="409">Un vehículo vendido no se puede eliminar.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Politicas.SoloDueno)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Eliminar(int id) =>
            Responder(_vehiculos.Eliminar(id, Actor), NoContent);
    }
}
