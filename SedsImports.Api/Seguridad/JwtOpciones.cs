using System.ComponentModel.DataAnnotations;

namespace SedsImports.Api.Seguridad
{
    /// <summary>
    /// Configuración del token JWT (sección "Jwt" de appsettings). La clave de firma NO se guarda en
    /// el repositorio para producción: se toma de la variable de entorno Jwt__Clave o de los
    /// secretos de usuario (dotnet user-secrets). Se valida al iniciar la aplicación.
    /// </summary>
    public class JwtOpciones
    {
        public const string Seccion = "Jwt";

        [Required] public string Emisor { get; set; } = string.Empty;
        [Required] public string Audiencia { get; set; } = string.Empty;

        /// <summary>Clave secreta para firmar el token con HMAC-SHA256: mínimo 32 caracteres.</summary>
        [Required, MinLength(32, ErrorMessage = "La clave JWT debe tener al menos 32 caracteres.")]
        public string Clave { get; set; } = string.Empty;

        /// <summary>Minutos de validez del token.</summary>
        [Range(5, 1440)] public int MinutosExpiracion { get; set; } = 60;
    }
}
