using System.ComponentModel.DataAnnotations;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Persona interna que usa el sistema (tabla Usuarios). Pertenece a un rol.</summary>
    public class Usuario : IEntidad
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>Usuario de acceso (correo corporativo). Es único.</summary>
        [Required, EmailAddress, StringLength(80)]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Hash SHA-256 de la contraseña. La seguridad completa se implementa en la Fase 4.</summary>
        [Required, StringLength(64)]
        public string ContrasenaHash { get; set; } = string.Empty;

        /// <summary>Llave foránea hacia Roles.</summary>
        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public bool Activo { get; set; } = true;

        // ── Propiedades calculadas: no se guardan en la base de datos ──

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
