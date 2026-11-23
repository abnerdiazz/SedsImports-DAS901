using System.ComponentModel.DataAnnotations;

namespace SedsImports.Application.DTOs.Api
{
    /// <summary>Revisión y preparación de un vehículo con sus anotaciones.</summary>
    public class RevisionDto
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public string? Mecanico { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string? ResumenFinal { get; set; }
        public string? Pendientes { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
        public string? AutorizadoPor { get; set; }
        public List<NotaDto> Notas { get; set; } = new();
    }

    /// <summary>Anotación de la revisión.</summary>
    public class NotaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public bool EsDelSistema { get; set; }
    }

    /// <summary>Nueva anotación (POST /api/vehiculos/{id}/revision/notas).</summary>
    public class CrearNotaDto
    {
        /// <example>Cambio de aceite y filtros realizado.</example>
        [Required(ErrorMessage = "Escribe la anotación.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "La anotación debe tener entre {2} y {1} caracteres.")]
        public string Texto { get; set; } = string.Empty;
    }

    /// <summary>Cierre de la revisión (PUT /api/vehiculos/{id}/revision/finalizacion).</summary>
    public class FinalizarRevisionDto
    {
        /// <example>Cambio de aceite, filtros y bujías. Limpieza de inyectores.</example>
        [Required(ErrorMessage = "Escribe el resumen de los trabajos realizados.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "El resumen debe tener entre {2} y {1} caracteres.")]
        public string ResumenFinal { get; set; } = string.Empty;

        /// <example>Falta la llanta de repuesto.</example>
        [StringLength(500, ErrorMessage = "Los pendientes admiten máximo {1} caracteres.")]
        public string? Pendientes { get; set; }
    }

    /// <summary>Regreso del vehículo a revisión con el motivo (POST /api/vehiculos/{id}/revision/regreso).</summary>
    public class RegresarRevisionDto
    {
        /// <example>Se detectó una fuga de aceite en la prueba de manejo.</example>
        [Required(ErrorMessage = "Indica el motivo.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre {2} y {1} caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
