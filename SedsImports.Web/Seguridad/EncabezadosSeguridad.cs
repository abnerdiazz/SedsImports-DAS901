namespace SedsImports.Web.Seguridad
{
    /// <summary>
    /// Encabezados HTTP de seguridad para todas las páginas:
    /// Content-Security-Policy limita de dónde se cargan scripts, estilos e imágenes (mitiga XSS),
    /// X-Frame-Options evita que el sitio se muestre dentro de un iframe (clickjacking) y
    /// X-Content-Type-Options evita que el navegador interprete archivos con otro tipo.
    /// </summary>
    public static class EncabezadosSeguridad
    {
        private const string Csp =
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: blob:; " +
            "connect-src 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "object-src 'none'";

        public static IApplicationBuilder UsarEncabezadosSeguridad(this IApplicationBuilder app) =>
            app.Use(async (http, siguiente) =>
            {
                var h = http.Response.Headers;
                h["Content-Security-Policy"] = Csp;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                h.Remove("X-Powered-By");
                await siguiente();
            });
    }
}
