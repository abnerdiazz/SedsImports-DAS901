using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Web.Servicios.Api
{
    /// <summary>
    /// Implementación con HttpClient (registrado con AddHttpClient). El token JWT lo agrega
    /// automáticamente el ManejadorTokenApi en cada petición, así este cliente solo se ocupa de
    /// armar la petición y leer la respuesta JSON.
    /// </summary>
    public class SedsApiCliente : ISedsApiCliente
    {
        public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _http;
        private readonly ILogger<SedsApiCliente> _log;

        public SedsApiCliente(HttpClient http, ILogger<SedsApiCliente> log)
        {
            _http = http;
            _log = log;
        }

        public Task<RespuestaApi<TokenDto>> IniciarSesionAsync(string correo, string contrasena, CancellationToken cancelacion = default) =>
            EnviarAsync<TokenDto>(HttpMethod.Post, "api/auth/login", new LoginDto { Correo = correo, Contrasena = contrasena }, cancelacion);

        public Task<RespuestaApi<ResumenDto>> ResumenAsync(CancellationToken cancelacion = default) =>
            EnviarAsync<ResumenDto>(HttpMethod.Get, "api/reportes/resumen", null, cancelacion);

        public Task<RespuestaApi<List<VehiculoListadoDto>>> VehiculosAsync(string? busqueda, EstadoVehiculo? estado, CancellationToken cancelacion = default)
        {
            var ruta = "api/vehiculos?busqueda=" + Uri.EscapeDataString(busqueda ?? string.Empty)
                       + (estado.HasValue ? "&estado=" + estado.Value : string.Empty);
            return EnviarAsync<List<VehiculoListadoDto>>(HttpMethod.Get, ruta, null, cancelacion);
        }

        public Task<RespuestaApi<NotaDto>> AgregarNotaAsync(int vehiculoId, string texto, CancellationToken cancelacion = default) =>
            EnviarAsync<NotaDto>(HttpMethod.Post, $"api/vehiculos/{vehiculoId}/revision/notas", new CrearNotaDto { Texto = texto }, cancelacion);

        private async Task<RespuestaApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken cancelacion)
        {
            try
            {
                using var peticion = new HttpRequestMessage(metodo, ruta);
                if (cuerpo != null) peticion.Content = JsonContent.Create(cuerpo, options: Json);

                using var respuesta = await _http.SendAsync(peticion, cancelacion);
                if (respuesta.IsSuccessStatusCode)
                {
                    var dato = await respuesta.Content.ReadFromJsonAsync<T>(Json, cancelacion);
                    return RespuestaApi<T>.Ok(dato!, (int)respuesta.StatusCode);
                }

                return await LeerErrorAsync<T>(respuesta, cancelacion);
            }
            catch (HttpRequestException ex)
            {
                _log.LogError(ex, "No se pudo conectar con la API en {Ruta}.", ruta);
                return RespuestaApi<T>.Falla(503, "No se pudo conectar con el servicio (API). Verifica que la API esté en ejecución.");
            }
            catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
            {
                // El navegador canceló la petición (por ejemplo, cambió de página): no es un error.
                return RespuestaApi<T>.Falla(499, "Solicitud cancelada.");
            }
            catch (TaskCanceledException)
            {
                _log.LogWarning("La API tardó demasiado en responder ({Ruta}).", ruta);
                return RespuestaApi<T>.Falla(504, "El servicio tardó demasiado en responder. Intenta de nuevo.");
            }
        }

        /// <summary>Lee el ProblemDetails de la API para mostrar un mensaje claro al usuario.</summary>
        private async Task<RespuestaApi<T>> LeerErrorAsync<T>(HttpResponseMessage respuesta, CancellationToken cancelacion)
        {
            var codigo = (int)respuesta.StatusCode;
            var mensaje = respuesta.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión venció. Vuelve a iniciar sesión.",
                HttpStatusCode.Forbidden => "Tu perfil no tiene permiso para esta acción.",
                HttpStatusCode.TooManyRequests => "Demasiados intentos. Espera un minuto e intenta de nuevo.",
                HttpStatusCode.RequestTimeout => "El servicio tardó demasiado en responder. Intenta de nuevo.",
                HttpStatusCode.ServiceUnavailable => "No se pudo conectar con el servicio (API). Verifica que la API esté en ejecución.",
                _ => "No se pudo completar la operación."
            };
            var errores = new Dictionary<string, string[]>();

            try
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ProblemaApi>(Json, cancelacion);
                if (!string.IsNullOrWhiteSpace(problema?.Detail)) mensaje = problema.Detail;
                if (problema?.Errors != null)
                {
                    errores = problema.Errors;
                    mensaje = problema.Errors.Values.SelectMany(v => v).FirstOrDefault() ?? mensaje;
                }
            }
            catch (JsonException)
            {
                // La respuesta no traía JSON: se usa el mensaje según el código.
            }

            _log.LogWarning("La API respondió {Codigo} en {Ruta}: {Mensaje}", codigo, respuesta.RequestMessage?.RequestUri?.PathAndQuery, mensaje);
            return RespuestaApi<T>.Falla(codigo, mensaje, errores);
        }

        private sealed class ProblemaApi
        {
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }
}
