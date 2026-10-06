using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>Reserva de un vehículo disponible con un anticipo (RF-09, CU-05).</summary>
    public class Apartado : IEntidad
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }

        public DateTime FechaApartado { get; set; } = DateTime.Now;

        [Range(0.01, 1_000_000, ErrorMessage = "El anticipo debe ser mayor que cero.")]
        public decimal Anticipo { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public EstadoApartado Estado { get; set; } = EstadoApartado.Activo;

        public string RegistradoPor { get; set; } = string.Empty;

        public DateTime? FechaCierre { get; set; }
        public string? MotivoCancelacion { get; set; }
    }
}
