using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>
    /// Entidad central del sistema (tabla Vehiculos). Todo el proceso (revisión, documentación,
    /// apartado y venta) gira alrededor de un vehículo.
    /// </summary>
    public class Vehiculo : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Número de identificación del vehículo. Es único entre los vehículos no eliminados.</summary>
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

        /// <summary>Llave foránea hacia Usuarios: quién registró el vehículo.</summary>
        public int RegistradoPorId { get; set; }

        /// <summary>Nombre del usuario al momento del registro (dato histórico para la ficha).</summary>
        [StringLength(80)]
        public string RegistradoPorNombre { get; set; } = string.Empty;

        public DateTime? FechaUltimaEdicion { get; set; }

        [StringLength(80)]
        public string? EditadoPorNombre { get; set; }

        /// <summary>
        /// Eliminación lógica: el vehículo deja de mostrarse en el sistema,
        /// pero sus datos y su historial se conservan.
        /// </summary>
        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        [StringLength(80)]
        public string? EliminadoPorNombre { get; set; }

        // ── Relaciones ──

        /// <summary>1 a N: fotografías del vehículo en sus tres momentos (ingreso, revisión y finales).</summary>
        public List<Fotografia> Fotografias { get; set; } = new();

        /// <summary>1 a 1: revisión y preparación del mecánico.</summary>
        public Revision? Revision { get; set; }

        /// <summary>1 a 1: expediente de documentos.</summary>
        public Documentacion? Documentacion { get; set; }

        /// <summary>1 a N: apartados (activos, cancelados o convertidos en venta).</summary>
        public List<Apartado> Apartados { get; set; } = new();

        /// <summary>1 a 0..1: venta final del vehículo.</summary>
        public Venta? Venta { get; set; }

        // ── Propiedades calculadas: no se guardan en la base de datos ──

        public string NombreCompleto => $"{Marca} {Modelo} {Anio}";

        public string VinCorto => Vin.Length > 6 ? "..." + Vin[^6..] : Vin;
    }
}
