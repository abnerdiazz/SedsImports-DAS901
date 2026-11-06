using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Ventas: 1 a 0..1 con Vehiculos, N a 1 con Clientes y 0..1 con Apartados.</summary>
    public class VentaConfiguracion : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas", t =>
                t.HasCheckConstraint("CK_Ventas_Monto", "[Monto] IS NULL OR [Monto] >= 0"));
            builder.HasKey(v => v.Id);

            builder.Property(v => v.FechaVenta).HasColumnType("date");
            builder.Property(v => v.RegistradoPor).IsRequired().HasMaxLength(80);

            // Un vehículo se vende una sola vez: índice único sobre VehiculoId.
            builder.HasOne(v => v.Vehiculo)
                   .WithOne(x => x.Venta)
                   .HasForeignKey<Venta>(v => v.VehiculoId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(v => v.VehiculoId).IsUnique();

            builder.HasOne(v => v.Cliente)
                   .WithMany(c => c.Ventas)
                   .HasForeignKey(v => v.ClienteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Apartado)
                   .WithMany()
                   .HasForeignKey(v => v.ApartadoId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Para el reporte mensual.
            builder.HasIndex(v => v.FechaVenta);
        }
    }
}
