using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Reserva de un vehículo disponible con un anticipo (tabla Apartados, RF-09, CU-05).</summary>
    public class Apartado : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Vehiculos.</summary>
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        /// <summary>Llave foránea hacia Clientes.</summary>
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public DateTime FechaApartado { get; set; } = DateTime.Now;

        [Range(0.01, 1_000_000, ErrorMessage = "El anticipo debe ser mayor que cero.")]
        public decimal Anticipo { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public EstadoApartado Estado { get; set; } = EstadoApartado.Activo;

        [StringLength(80)]
        public string RegistradoPor { get; set; } = string.Empty;

        public DateTime? FechaCierre { get; set; }

        [StringLength(300)]
        public string? MotivoCancelacion { get; set; }
    }
}
