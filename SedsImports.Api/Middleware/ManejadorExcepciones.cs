using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Domain.Excepciones;

namespace SedsImports.Api.Middleware
{
    /// <summary>
    /// Manejador global de excepciones: registra el error en el log con el identificador de la
    /// petición y responde siempre con ProblemDetails (JSON), sin mostrar detalles internos al
    /// cliente. Así ningún error deja la API sin respuesta ni expone la pila de llamadas.
    /// </summary>
    public class ManejadorExcepciones : IExceptionHandler
    {
        private readonly ILogger<ManejadorExcepciones> _log;
        private readonly IProblemDetailsService _problemas;
        private readonly IHostEnvironment _entorno;

        public ManejadorExcepciones(ILogger<ManejadorExcepciones> log, IProblemDetailsService problemas, IHostEnvironment entorno)
        {
            _log = log;
            _problemas = problemas;
            _entorno = entorno;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excepcion, CancellationToken cancelacion)
        {
            var traza = Activity.Current?.Id ?? http.TraceIdentifier;

            var (codigo, titulo, detalle) = excepcion switch
            {
                // Error al guardar (llave duplicada, restricción CHECK, conexión): ya trae un mensaje claro.
                ErrorPersistenciaException e => (StatusCodes.Status409Conflict, "No se pudo guardar", e.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autenticado", "El token no corresponde a un usuario activo."),
                BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud no válida", "El formato de la solicitud no es válido."),
                OperationCanceledException => (499, "Solicitud cancelada", "El cliente canceló la solicitud."),
                _ => (StatusCodes.Status500InternalServerError, "Error interno",
                      "Ocurrió un error inesperado. Si continúa, comunícate con el administrador e indica el código de seguimiento.")
            };

            if (codigo >= 500)
                _log.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}. Traza {Traza}", http.Request.Method, http.Request.Path, traza);
            else
                _log.LogWarning("Excepción controlada {Tipo} en {Metodo} {Ruta}: {Mensaje}. Traza {Traza}",
                    excepcion.GetType().Name, http.Request.Method, http.Request.Path, excepcion.Message, traza);

            http.Response.StatusCode = codigo;
            var problema = new ProblemDetails
            {
                Status = codigo,
                Title = titulo,
                Detail = detalle,
                Instance = http.Request.Path
            };
            problema.Extensions["traceId"] = traza;

            // Solo en desarrollo se agrega el tipo de excepción, para depurar.
            if (_entorno.IsDevelopment()) problema.Extensions["excepcion"] = excepcion.GetType().Name;

            return await _problemas.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = http,
                ProblemDetails = problema,
                Exception = excepcion
            });
        }
    }
}
