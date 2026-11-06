using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>
    /// Estado documental del vehículo (tabla Documentaciones, RF-07, CU-04). Es un proceso paralelo:
    /// la asistente lo actualiza desde que el vehículo llega, sin esperar la revisión.
    /// </summary>
    public class Documentacion : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Vehiculos. Es única: un expediente por vehículo.</summary>
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        public bool TieneTitulo { get; set; }
        public bool TieneImportacion { get; set; }
        public bool TieneTarjetaCirculacion { get; set; }
        public bool TieneTraspaso { get; set; }
        public bool TieneOtros { get; set; }

        public EstadoDocumentacion Estado { get; set; } = EstadoDocumentacion.Pendiente;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        [StringLength(80)]
        public string? ActualizadoPor { get; set; }

        // ── Propiedades calculadas: no se guardan en la base de datos ──

        /// <summary>Los cuatro documentos principales que exige la venta.</summary>
        public bool DocumentosPrincipalesCompletos =>
            TieneTitulo && TieneImportacion && TieneTarjetaCirculacion && TieneTraspaso;

        public int CantidadDocumentos =>
            new[] { TieneTitulo, TieneImportacion, TieneTarjetaCirculacion, TieneTraspaso, TieneOtros }.Count(x => x);
    }
}
