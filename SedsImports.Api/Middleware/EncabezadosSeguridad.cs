namespace SedsImports.Api.Middleware
{
    /// <summary>
    /// Agrega encabezados HTTP de seguridad a todas las respuestas:
    /// evita que el navegador "adivine" el tipo de contenido, que la API se muestre dentro de
    /// un iframe (clickjacking) y que se filtre la dirección de origen.
    /// </summary>
    public static class EncabezadosSeguridad
    {
        public static IApplicationBuilder UsarEncabezadosSeguridad(this IApplicationBuilder app) =>
            app.Use(async (http, siguiente) =>
            {
                var h = http.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "no-referrer";
                h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                // Swagger UI necesita sus propios scripts y estilos; el resto de la API solo devuelve JSON.
                if (!http.Request.Path.StartsWithSegments("/swagger"))
                    h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                h.Remove("Server");
                h.Remove("X-Powered-By");
                await siguiente();
            });
    }
}
