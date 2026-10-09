using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>Persona interna que usa el sistema. Pertenece a un rol.</summary>
    public class Usuario : IEntidad
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>Usuario de acceso (correo corporativo).</summary>
        [Required, EmailAddress, StringLength(80)]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Hash SHA-256 de la contraseña. La seguridad completa se implementa en la Fase 4.</summary>
        public string ContrasenaHash { get; set; } = string.Empty;

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public bool Activo { get; set; } = true;

        public string Iniciales
        {
            get
            {
                var partes = NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return partes.Length switch
                {
                    0 => "?",
                    1 => partes[0][..1].ToUpper(),
                    _ => (partes[0][..1] + partes[1][..1]).ToUpper()
                };
            }
        }

        public string PrimerNombre => NombreCompleto.Split(' ')[0];
    }
}
