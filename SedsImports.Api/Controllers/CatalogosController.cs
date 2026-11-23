using Microsoft.AspNetCore.Mvc;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Mapeos;
using SedsImports.Domain.Enums;

namespace SedsImports.Api.Controllers
{
    /// <summary>Catálogos del sistema para llenar listas desplegables en los clientes de la API.</summary>
    [Route("api/catalogos")]
    public class CatalogosController : ApiControllerBase
    {
        /// <summary>Estados por los que pasa un vehículo.</summary>
        [HttpGet("estados-vehiculo")]
        [ProducesResponseType(typeof(List<CatalogoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<CatalogoDto>> EstadosVehiculo() => Ok(VehiculoMapeos.ACatalogo<EstadoVehiculo>());

        /// <summary>Estados de la documentación.</summary>
        [HttpGet("estados-documentacion")]
        [ProducesResponseType(typeof(List<CatalogoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<CatalogoDto>> EstadosDocumentacion() => Ok(VehiculoMapeos.ACatalogo<EstadoDocumentacion>());

        /// <summary>Roles del sistema.</summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<CatalogoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<CatalogoDto>> Roles() => Ok(VehiculoMapeos.ACatalogo<TipoRol>());

        /// <summary>Tipos de movimiento de la bitácora.</summary>
        [HttpGet("tipos-movimiento")]
        [ProducesResponseType(typeof(List<CatalogoDto>), StatusCodes.Status200OK)]
        public ActionResult<List<CatalogoDto>> TiposMovimiento() => Ok(VehiculoMapeos.ACatalogo<TipoMovimiento>());
    }
}
