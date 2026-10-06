namespace SedsImports.Web.Models
{
    /// <summary>Venta final de un vehículo (RF-10, CU-06). Alimenta el reporte mensual (RF-12, CU-08).</summary>
    public class Venta : IEntidad
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }
        public int? ApartadoId { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Today;

        /// <summary>Monto de la venta en USD. Es opcional (el cálculo de precios está fuera del alcance).</summary>
        public decimal? Monto { get; set; }

        public string RegistradoPor { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
