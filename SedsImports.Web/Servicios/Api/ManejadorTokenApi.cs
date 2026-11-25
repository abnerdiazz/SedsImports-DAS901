using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace SedsImports.Web.Servicios.Api
{
    /// <summary>
    /// Agrega el token JWT del usuario a cada petición hacia la API (Authorization: Bearer).
    /// El token se guarda dentro de la cookie de autenticación, que está cifrada y es HttpOnly:
    /// el JavaScript del navegador nunca lo ve, lo que protege el token ante ataques XSS.
    /// Además convierte los problemas de conexión en respuestas HTTP (408 y 503), para que la
    /// aplicación muestre un mensaje claro en lugar de un error.
    /// </summary>
    public class ManejadorTokenApi : DelegatingHandler
    {
        public const string NombreToken = "access_token";

        private readonly IHttpContextAccessor _http;

        public ManejadorTokenApi(IHttpContextAccessor http) => _http = http;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
        {
            var contexto = _http.HttpContext;
            if (contexto != null && peticion.Headers.Authorization == null)
            {
                var token = await contexto.GetTokenAsync(NombreToken);
                if (!string.IsNullOrEmpty(token))
                    peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                return await base.SendAsync(peticion, cancelacion);
            }
            catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
            {
                // El navegador canceló la petición (cambió de página o escribió otra búsqueda)
                // o se agotó el tiempo de espera: se responde 408 en lugar de propagar la excepción.
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout) { RequestMessage = peticion };
            }
            catch (HttpRequestException)
            {
                // La API no está en ejecución o no se pudo conectar: se responde 503.
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { RequestMessage = peticion };
            }
        }
    }
}
