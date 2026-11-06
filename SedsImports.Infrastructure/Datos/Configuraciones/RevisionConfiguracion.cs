using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Revisiones: relación 1 a 1 con Vehiculos.</summary>
    public class RevisionConfiguracion : IEntityTypeConfiguration<Revision>
    {
        public void Configure(EntityTypeBuilder<Revision> builder)
        {
            builder.ToTable("Revisiones");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.MecanicoNombre).HasMaxLength(80);
            builder.Property(r => r.ResumenFinal).HasMaxLength(800);
            builder.Property(r => r.Pendientes).HasMaxLength(800);
            builder.Property(r => r.AutorizadoPor).HasMaxLength(80);

            // 1 a 1: el índice único sobre VehiculoId garantiza una sola revisión por vehículo.
            builder.HasOne(r => r.Vehiculo)
                   .WithOne(v => v.Revision)
                   .HasForeignKey<Revision>(r => r.VehiculoId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(r => r.VehiculoId).IsUnique();

            // Mecánico que inició la revisión (opcional).
            builder.HasOne<Usuario>()
                   .WithMany()
                   .HasForeignKey(r => r.MecanicoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
