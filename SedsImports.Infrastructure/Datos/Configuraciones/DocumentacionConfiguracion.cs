using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Documentaciones: relación 1 a 1 con Vehiculos.</summary>
    public class DocumentacionConfiguracion : IEntityTypeConfiguration<Documentacion>
    {
        public void Configure(EntityTypeBuilder<Documentacion> builder)
        {
            builder.ToTable("Documentaciones");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Estado).IsRequired();
            builder.Property(d => d.Observaciones).HasMaxLength(500);
            builder.Property(d => d.ActualizadoPor).HasMaxLength(80);

            builder.HasOne(d => d.Vehiculo)
                   .WithOne(v => v.Documentacion)
                   .HasForeignKey<Documentacion>(d => d.VehiculoId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(d => d.VehiculoId).IsUnique();

            builder.Ignore(d => d.DocumentosPrincipalesCompletos);
            builder.Ignore(d => d.CantidadDocumentos);
        }
    }
}
