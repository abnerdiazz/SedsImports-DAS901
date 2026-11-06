using System.ComponentModel.DataAnnotations;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Venta final de un vehículo (tabla Ventas, RF-10, CU-06). Alimenta el reporte mensual (RF-12, CU-08).</summary>
    public class Venta : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Llave foránea hacia Vehiculos. Es única: un vehículo se vende una sola vez.</summary>
        public int VehiculoId { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        /// <summary>Llave foránea hacia Clientes.</summary>
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        /// <summary>Llave foránea (opcional) hacia Apartados: el apartado que se convirtió en esta venta.</summary>
        public int? ApartadoId { get; set; }
        public Apartado? Apartado { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Today;

        /// <summary>Monto de la venta en USD. Es opcional (el cálculo de precios está fuera del alcance).</summary>
        public decimal? Monto { get; set; }

        [StringLength(80)]
        public string RegistradoPor { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
