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
    /// <summary>Reportes e indicadores (consultas LINQ de la capa de infraestructura).</summary>
    [Route("api/reportes")]
    public class ReportesController : ApiControllerBase
    {
        private readonly IVehiculoService _vehiculos;
        private readonly IDocumentacionService _documentacion;
        private readonly IApartadoService _apartados;
        private readonly IVentaService _ventas;

        public ReportesController(IVehiculoService vehiculos, IDocumentacionService documentacion,
            IApartadoService apartados, IVentaService ventas)
        {
            _vehiculos = vehiculos;
            _documentacion = documentacion;
            _apartados = apartados;
            _ventas = ventas;
        }

        /// <summary>Indicadores del panel de inicio según el rol del usuario.</summary>
        /// <remarks>Lo usa la aplicación web para actualizar las tarjetas sin recargar la página (Fetch API).</remarks>
        [HttpGet("resumen")]
        [ProducesResponseType(typeof(ResumenDto), StatusCodes.Status200OK)]
        public ActionResult<ResumenDto> Resumen()
        {
            var rol = RolActor;
            var resumen = new ResumenDto
            {
                Vehiculos = _vehiculos.Conteos(Permisos.EstadosVisibles(rol)).ADto(),
                RegistradosHoy = _vehiculos.RegistradosDesde(DateTime.Today)
            };

            // Los datos comerciales y de documentación solo se envían a quien los puede ver.
            if (rol is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor)
            {
                var (pendientes, completas) = _documentacion.Conteos();
                var (vendidos, total) = _ventas.VentasDelMes(DateTime.Today);
                resumen.DocumentacionPendiente = pendientes;
                resumen.DocumentacionCompleta = completas;
                resumen.VendidosMes = vendidos;
                resumen.VentasMes = total;
                resumen.ApartadosActivos = _apartados.ListarActivos().Count;
            }

            return Ok(resumen);
        }

        /// <summary>Reporte de ventas por mes, por marca y detalle (año obligatorio; mes, día y búsqueda opcionales).</summary>
        /// <param name="anio">Año del reporte.</param>
        /// <param name="mes">Mes (1-12).</param>
        /// <param name="dia">Día del mes.</param>
        /// <param name="busqueda">Cliente o vehículo.</param>
        /// <response code="200">Reporte de ventas.</response>
        /// <response code="400">Mes o día fuera de rango.</response>
        [HttpGet("ventas")]
        [Authorize(Policy = Politicas.SoloDueno)]
        [ProducesResponseType(typeof(ReporteVentasDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<ReporteVentasDto> Ventas([FromQuery] int? anio, [FromQuery] int? mes, [FromQuery] int? dia, [FromQuery] string? busqueda)
        {
            if (mes is < 1 or > 12) ModelState.AddModelError(nameof(mes), "El mes debe estar entre 1 y 12.");
            if (dia is < 1 or > 31) ModelState.AddModelError(nameof(dia), "El día debe estar entre 1 y 31.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            return Ok(_ventas.Reporte(anio ?? DateTime.Today.Year, mes, dia, busqueda));
        }

        /// <summary>Años que tienen ventas registradas (para el filtro del reporte).</summary>
        [HttpGet("anios")]
        [Authorize(Policy = Politicas.SoloDueno)]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public ActionResult<List<int>> Anios() => Ok(_ventas.AniosConVentas());
    }
}
