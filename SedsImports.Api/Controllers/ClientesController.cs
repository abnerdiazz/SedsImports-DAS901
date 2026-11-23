using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.Mapeos;

namespace SedsImports.Api.Controllers
{
    /// <summary>Clientes que apartan o compran vehículos (los crea el registro de apartados y ventas).</summary>
    [Route("api/clientes")]
    [Authorize(Policy = Politicas.SoloDueno)]
    public class ClientesController : ApiControllerBase
    {
        private readonly IClienteService _clientes;

        public ClientesController(IClienteService clientes) => _clientes = clientes;

        /// <summary>Lista los clientes; filtra por nombre o teléfono.</summary>
        /// <response code="200">Clientes ordenados por nombre.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
        public ActionResult<List<ClienteDto>> Listar([FromQuery] string? busqueda) =>
            Ok(_clientes.Listar(busqueda).Select(c => c.ADto()).ToList());

        /// <summary>Datos de un cliente.</summary>
        /// <response code="200">Cliente encontrado.</response>
        /// <response code="404">El cliente no existe.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ClienteDto> Obtener(int id)
        {
            var cliente = _clientes.Obtener(id);
            return cliente == null ? NoEncontrado("El cliente no existe.") : Ok(cliente.ADto());
        }
    }
}
