using System.ComponentModel.DataAnnotations;

namespace SedsImports.Domain.Entidades
{
    /// <summary>
    /// Registro del proceso de revisión y preparación que hace el mecánico (tabla Revisiones, RF-05, CU-02).
    /// Al finalizar, el vehículo NO queda disponible automáticamente: el dueño debe autorizarlo (CU-03).
    /// </summary>
    public class Revision : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Vehiculos. Es única: un vehículo tiene una sola revisión.</summary>
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        /// <summary>Llave foránea (opcional) hacia Usuarios: el mecánico que inició la revisión.</summary>
        public int? MecanicoId { get; set; }

        [StringLength(80)]
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

        [StringLength(80)]
        public string? AutorizadoPor { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        /// <summary>1 a N: anotaciones del mecánico.</summary>
        public List<NotaRevision> Notas { get; set; } = new();
    }
}
