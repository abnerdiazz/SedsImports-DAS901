using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Apartados: relación N a 1 con Vehiculos y con Clientes.</summary>
    public class ApartadoConfiguracion : IEntityTypeConfiguration<Apartado>
    {
        public void Configure(EntityTypeBuilder<Apartado> builder)
        {
            builder.ToTable("Apartados", t =>
                t.HasCheckConstraint("CK_Apartados_Anticipo", "[Anticipo] > 0"));
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Anticipo).IsRequired();
            builder.Property(a => a.Estado).IsRequired();
            builder.Property(a => a.Observaciones).HasMaxLength(500);
            builder.Property(a => a.RegistradoPor).IsRequired().HasMaxLength(80);
            builder.Property(a => a.MotivoCancelacion).HasMaxLength(300);

            builder.HasOne(a => a.Vehiculo)
                   .WithMany(v => v.Apartados)
                   .HasForeignKey(a => a.VehiculoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Cliente)
                   .WithMany(c => c.Apartados)
                   .HasForeignKey(a => a.ClienteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.VehiculoId, a.Estado });
        }
    }
}
