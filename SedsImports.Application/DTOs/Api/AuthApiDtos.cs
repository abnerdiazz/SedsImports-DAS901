using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.DTOs.Api
{
    /// <summary>Credenciales para iniciar sesión (POST /api/auth/login).</summary>
    public class LoginDto
    {
        /// <example>ernesto.rivas@sedimports.com</example>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido.")]
        [StringLength(80)]
        public string Correo { get; set; } = string.Empty;

        /// <example>Seds2026</example>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100)]
        public string Contrasena { get; set; } = string.Empty;
    }

    /// <summary>Respuesta del inicio de sesión: el token JWT y los datos del usuario.</summary>
    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Bearer";
        public DateTime Expira { get; set; }
        public UsuarioDto Usuario { get; set; } = new();
    }

    /// <summary>Usuario del sistema (sin datos sensibles: nunca se devuelve la contraseña ni su hash).</summary>
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public TipoRol Rol { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    /// <summary>Nuevo usuario (POST /api/usuarios). El correo se genera con el nombre y el apellido.</summary>
    public class CrearUsuarioDto
    {
        /// <example>Lucía</example>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(40, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <example>Mena</example>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(40, ErrorMessage = "El apellido admite máximo {1} caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        /// <example>Mecanico</example>
        [Required(ErrorMessage = "Selecciona el rol.")]
        public TipoRol? Rol { get; set; }

        /// <example>Taller2026</example>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe combinar letras y números.")]
        public string Contrasena { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
