using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>
    /// Registro del proceso de revisión y preparación que hace el mecánico (RF-05, CU-02).
    /// Al finalizar, el vehículo NO queda disponible automáticamente: el dueño debe autorizarlo (CU-03).
    /// </summary>
    public class Revision : IEntidad
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }

        public int? MecanicoId { get; set; }
        public string? MecanicoNombre { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        /// <summary>Resumen final de los trabajos que hizo el mecánico.</summary>
        [StringLength(800)]
        public string? ResumenFinal { get; set; }

        /// <summary>Últimos apuntes: lo que todavía le falta al vehículo (para que el dueño decida).</summary>
        [StringLength(800)]
        public string? Pendientes { get; set; }

        public DateTime? FechaAutorizacion { get; set; }
        public string? AutorizadoPor { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public List<NotaRevision> Notas { get; set; } = new();
    }

    /// <summary>Anotación que el mecánico agrega mientras trabaja el vehículo.</summary>
    public class NotaRevision
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required, StringLength(500)]
        public string Texto { get; set; } = string.Empty;

        /// <summary>Marca las notas generadas por el sistema (finalización, regreso a revisión, etc.).</summary>
        public bool EsDelSistema { get; set; }
    }
}
