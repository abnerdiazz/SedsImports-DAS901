using System.ComponentModel.DataAnnotations;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Persona interesada en un vehículo; se asocia a apartados y ventas (tabla Clientes, RF-08).</summary>
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

        /// <summary>1 a N: apartados del cliente.</summary>
        public List<Apartado> Apartados { get; set; } = new();

        /// <summary>1 a N: compras del cliente.</summary>
        public List<Venta> Ventas { get; set; } = new();
    }
}
