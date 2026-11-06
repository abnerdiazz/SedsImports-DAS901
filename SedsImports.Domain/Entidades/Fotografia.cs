using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Fotografía asociada a un vehículo (tabla Fotografias).</summary>
    public class Fotografia : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Vehiculos.</summary>
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        public TipoFoto Tipo { get; set; }

        /// <summary>Ruta web del archivo (null en los datos de ejemplo: se muestra un marcador de color).</summary>
        [StringLength(260)]
        public string? Ruta { get; set; }

        [StringLength(120)]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(80)]
        public string SubidaPor { get; set; } = string.Empty;
    }
}
