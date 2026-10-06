using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>Persona interesada en un vehículo; se asocia a un apartado o a una venta (RF-08).</summary>
    public class Cliente : IEntidad
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [StringLength(80)]
        public string? Correo { get; set; }
    }
}
