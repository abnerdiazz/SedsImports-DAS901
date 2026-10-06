using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>
    /// Estado documental del vehículo (RF-07, CU-04). Es un proceso paralelo:
    /// la asistente lo actualiza desde que el vehículo llega, sin esperar la revisión.
    /// </summary>
    public class Documentacion : IEntidad
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }

        public bool TieneTitulo { get; set; }
        public bool TieneImportacion { get; set; }
        public bool TieneTarjetaCirculacion { get; set; }
        public bool TieneTraspaso { get; set; }
        public bool TieneOtros { get; set; }

        public EstadoDocumentacion Estado { get; set; } = EstadoDocumentacion.Pendiente;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime? FechaActualizacion { get; set; }
        public string? ActualizadoPor { get; set; }

        /// <summary>Los cuatro documentos principales que exige la venta.</summary>
        public bool DocumentosPrincipalesCompletos =>
            TieneTitulo && TieneImportacion && TieneTarjetaCirculacion && TieneTraspaso;

        public int CantidadDocumentos =>
            new[] { TieneTitulo, TieneImportacion, TieneTarjetaCirculacion, TieneTraspaso, TieneOtros }.Count(x => x);
    }
}
