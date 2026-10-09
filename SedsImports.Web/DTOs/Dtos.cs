using SedsImports.Web.Models;

namespace SedsImports.Web.DTOs
{
    /// <summary>
    /// DTO plano para los listados de vehículos. Junta en una sola fila los datos del
    /// vehículo, su documentación y la siguiente acción, sin exponer las entidades completas.
    /// </summary>
    public class VehiculoListadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string VinCorto { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public EstadoVehiculo Estado { get; set; }
        public EstadoDocumentacion EstadoDocumentacion { get; set; }
        public string? ObservacionDocumentacion { get; set; }
        public string? SiguienteAccion { get; set; }
        public string? ResponsableSiguiente { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int CantidadNotas { get; set; }
        public int CantidadFotosFinales { get; set; }
        public int IndiceColor { get; set; }
    }

    /// <summary>Cantidad de vehículos en cada estado (chips de filtro y tarjetas del inicio).</summary>
    public class ConteoEstadosDto
    {
        public int Total { get; set; }
        public Dictionary<EstadoVehiculo, int> PorEstado { get; set; } = new();

        public int De(EstadoVehiculo estado) => PorEstado.TryGetValue(estado, out var n) ? n : 0;
    }

    /// <summary>Fila del reporte mensual de ventas (RF-12, CU-08).</summary>
    public class VentaMensualDto
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>Detalle de una venta para listados y reportes.</summary>
    public class VentaDetalleDto
    {
        public int VentaId { get; set; }
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? Monto { get; set; }
    }

    /// <summary>Resultado completo del reporte de ventas.</summary>
    public class ReporteVentasDto
    {
        public int Anio { get; set; }
        public List<VentaMensualDto> PorMes { get; set; } = new();
        public List<VentaDetalleDto> Detalle { get; set; } = new();
        public int TotalUnidades => Detalle.Count;
        public decimal TotalMonto => Detalle.Sum(d => d.Monto ?? 0);
    }

    /// <summary>Apartado activo con los datos del vehículo y del cliente.</summary>
    public class ApartadoListadoDto
    {
        public int ApartadoId { get; set; }
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public decimal Anticipo { get; set; }
        public DateTime Fecha { get; set; }
        public EstadoDocumentacion EstadoDocumentacion { get; set; }
    }
}
