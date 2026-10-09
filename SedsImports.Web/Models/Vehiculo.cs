using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>
    /// Entidad central del sistema. Todo el proceso (revisión, documentación,
    /// apartado y venta) gira alrededor de un vehículo.
    /// </summary>
    public class Vehiculo : IEntidad
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El VIN es obligatorio.")]
        [StringLength(17, MinimumLength = 11, ErrorMessage = "El VIN debe tener entre 11 y 17 caracteres.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "El VIN solo admite letras y números.")]
        public string Vin { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(40)]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(40)]
        public string Modelo { get; set; } = string.Empty;

        [Range(1950, 2100, ErrorMessage = "Ingresa un año válido.")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "El color es obligatorio.")]
        [StringLength(30)]
        public string Color { get; set; } = string.Empty;

        /// <summary>Detalles u observaciones generales al momento del ingreso.</summary>
        [StringLength(500)]
        public string? Detalles { get; set; }

        public EstadoVehiculo Estado { get; set; } = EstadoVehiculo.RecienLlegado;

        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public int RegistradoPorId { get; set; }
        public string RegistradoPorNombre { get; set; } = string.Empty;

        public DateTime? FechaUltimaEdicion { get; set; }
        public string? EditadoPorNombre { get; set; }

        /// <summary>Fotografías del vehículo en sus tres momentos (ingreso, revisión y finales).</summary>
        public List<Fotografia> Fotografias { get; set; } = new();

        public string NombreCompleto => $"{Marca} {Modelo} {Anio}";

        public string VinCorto => Vin.Length > 6 ? "..." + Vin[^6..] : Vin;
    }

    /// <summary>Fotografía asociada a un vehículo.</summary>
    public class Fotografia
    {
        public int Id { get; set; }
        public TipoFoto Tipo { get; set; }

        /// <summary>Ruta web del archivo (null en los datos de ejemplo: se muestra un marcador de color).</summary>
        public string? Ruta { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string SubidaPor { get; set; } = string.Empty;
    }
}
