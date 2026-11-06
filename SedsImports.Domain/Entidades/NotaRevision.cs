using System.ComponentModel.DataAnnotations;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Anotación que el mecánico agrega mientras trabaja el vehículo (tabla NotasRevision).</summary>
    public class NotaRevision : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Revisiones.</summary>
        public int RevisionId { get; set; }
        public Revision Revision { get; set; } = null!;

        /// <summary>Llave foránea hacia Usuarios: quién escribió la nota.</summary>
        public int UsuarioId { get; set; }

        [StringLength(80)]
        public string UsuarioNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required, StringLength(500)]
        public string Texto { get; set; } = string.Empty;

        /// <summary>Marca las notas generadas por el sistema (finalización, regreso a revisión, etc.).</summary>
        public bool EsDelSistema { get; set; }
    }
}
