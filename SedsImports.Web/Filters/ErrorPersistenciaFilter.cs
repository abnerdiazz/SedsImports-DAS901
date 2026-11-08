using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SedsImports.Domain.Excepciones;
using SedsImports.Web.Controllers;

namespace SedsImports.Web.Filters
{
    /// <summary>
    /// Filtro global de errores de base de datos. Si una operación lanza ErrorPersistenciaException
    /// (por ejemplo, se perdió la conexión o se violó una restricción), en lugar de mostrar una
    /// pantalla de error se regresa a la página anterior con un mensaje claro para el usuario.
    /// El detalle técnico queda en el log.
    /// </summary>
    public class ErrorPersistenciaFilter : IExceptionFilter
    {
        private readonly ITempDataDictionaryFactory _tempData;
        private readonly ILogger<ErrorPersistenciaFilter> _log;

        public ErrorPersistenciaFilter(ITempDataDictionaryFactory tempData, ILogger<ErrorPersistenciaFilter> log)
        {
            _tempData = tempData;
            _log = log;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ErrorPersistenciaException error) return;

            _log.LogError(error.InnerException ?? error, "Error al guardar en la base de datos: {Mensaje}", error.Message);

            var tempData = _tempData.GetTempData(context.HttpContext);
            tempData[BaseController.ClaveError] = error.Message;

            // Regresa a la página desde donde se envió el formulario; si no se conoce, al inicio.
            var origen = context.HttpContext.Request.Headers.Referer.ToString();
            context.Result = Uri.TryCreate(origen, UriKind.Absolute, out var url) && url.Host == context.HttpContext.Request.Host.Host
                ? new RedirectResult(url.PathAndQuery)
                : new RedirectToActionResult("Index", "Home", null);
            context.ExceptionHandled = true;
        }
    }
}
