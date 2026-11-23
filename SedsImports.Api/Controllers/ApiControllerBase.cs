using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Api.Controllers
{
    /// <summary>
    /// Base de todos los controladores de la API. [ApiController] valida automáticamente los DTOs
    /// (responde 400 con el detalle de cada campo) y [Authorize] exige un token JWT válido.
    /// Aquí también se traduce el resultado de los servicios a respuestas HTTP con ProblemDetails.
    /// </summary>
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>Usuario autenticado (viene del token). Los servicios lo usan como actor.</summary>
        protected Usuario Actor =>
            HttpContext.RequestServices.GetRequiredService<IUsuarioActualService>().Usuario
            ?? throw new UnauthorizedAccessException("El token no corresponde a un usuario activo.");

        protected TipoRol RolActor => Actor.Rol.Tipo;

        /// <summary>
        /// Convierte el resultado de un servicio en la respuesta HTTP adecuada:
        /// éxito → la respuesta indicada; sin permiso → 403; error de campo → 400;
        /// no existe → 404; regla del negocio no cumplida → 409.
        /// </summary>
        protected IActionResult Responder(ResultadoOperacion resultado, Func<IActionResult> exito)
        {
            if (resultado.Exitoso) return exito();

            if (resultado.Mensaje == Permisos.SinPermiso || resultado.Mensaje.StartsWith("Solo el "))
                return Problem(resultado.Mensaje, statusCode: StatusCodes.Status403Forbidden, title: "Acceso denegado");

            if (resultado.ErroresCampo.Count > 0)
            {
                foreach (var error in resultado.ErroresCampo)
                    ModelState.AddModelError(error.Key, error.Value);
                return ValidationProblem(ModelState);
            }

            if (resultado.Mensaje.Contains("no existe"))
                return Problem(resultado.Mensaje, statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Problem(resultado.Mensaje, statusCode: StatusCodes.Status409Conflict, title: "Regla del negocio");
        }

        /// <summary>404 con ProblemDetails y un mensaje claro.</summary>
        protected ObjectResult NoEncontrado(string mensaje) =>
            Problem(mensaje, statusCode: StatusCodes.Status404NotFound, title: "No encontrado");
    }
}
