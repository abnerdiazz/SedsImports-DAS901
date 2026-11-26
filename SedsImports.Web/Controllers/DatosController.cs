using Microsoft.AspNetCore.Mvc;
using SedsImports.Application.Comun;
using SedsImports.Domain.Enums;
using SedsImports.Web.Filters;
using SedsImports.Web.Servicios.Api;

namespace SedsImports.Web.Controllers
{
    /// <summary>
    /// Endpoints que llama el JavaScript de las vistas con Fetch API (comunicación asíncrona).
    /// Cada acción consume la API REST con el token del usuario y devuelve JSON o un fragmento
    /// HTML, para actualizar solo una parte de la página sin recargarla.
    /// Las llamadas a la API no se cancelan si el navegador cambia de página: terminan en milisegundos
    /// y así no se generan excepciones de cancelación.
    /// </summary>
    [Route("datos")]
    [PerfilRequerido]
    public class DatosController : BaseController
    {
        private readonly ISedsApiCliente _api;

        public DatosController(ISedsApiCliente api) => _api = api;

        // GET /datos/resumen → JSON con los indicadores del panel de inicio.
        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var respuesta = await _api.ResumenAsync();
            return respuesta.Exitosa ? Json(respuesta.Dato) : Error(respuesta.Codigo, respuesta.Error);
        }

        // GET /datos/vehiculos?busqueda=kia&estado=Disponible → filas de la tabla (vista parcial).
        [HttpGet("vehiculos")]
        public async Task<IActionResult> Vehiculos(string? busqueda, EstadoVehiculo? estado)
        {
            var respuesta = await _api.VehiculosAsync(busqueda, estado);
            if (!respuesta.Exitosa) return Error(respuesta.Codigo, respuesta.Error);

            ViewData["MostrarDocumentacion"] = RolActual is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor;
            Response.Headers["X-Total-Resultados"] = respuesta.Dato!.Count.ToString();
            return PartialView("~/Views/Vehiculos/_FilasVehiculos.cshtml", respuesta.Dato);
        }

        // POST /datos/vehiculos/5/notas  { "texto": "..." } → JSON con la anotación creada.
        // El encabezado X-CSRF-TOKEN se valida con el filtro antifalsificación global.
        [HttpPost("vehiculos/{vehiculoId:int}/notas")]
        [PerfilRequerido(TipoRol.Mecanico)]
        public async Task<IActionResult> AgregarNota(int vehiculoId, [FromBody] NotaEntrada entrada)
        {
            var texto = entrada?.Texto?.Trim() ?? string.Empty;
            if (texto.Length is < 3 or > 500)
                return Error(StatusCodes.Status400BadRequest, "La anotación debe tener entre 3 y 500 caracteres.");

            var respuesta = await _api.AgregarNotaAsync(vehiculoId, texto);
            return respuesta.Exitosa
                ? StatusCode(StatusCodes.Status201Created, new
                {
                    respuesta.Dato!.Id,
                    fecha = Formato.FechaHora(respuesta.Dato.Fecha),
                    respuesta.Dato.Usuario,
                    respuesta.Dato.Texto
                })
                : Error(respuesta.Codigo, respuesta.Error);
        }

        private ObjectResult Error(int codigo, string? mensaje) =>
            StatusCode(codigo, new { codigo, mensaje = mensaje ?? "No se pudo completar la operación." });

        public class NotaEntrada
        {
            public string? Texto { get; set; }
        }
    }
}
