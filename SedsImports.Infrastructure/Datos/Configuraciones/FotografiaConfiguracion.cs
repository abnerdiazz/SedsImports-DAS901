using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Fotografias: relación N a 1 con Vehiculos.</summary>
    public class FotografiaConfiguracion : IEntityTypeConfiguration<Fotografia>
    {
        public void Configure(EntityTypeBuilder<Fotografia> builder)
        {
            builder.ToTable("Fotografias");
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Tipo).IsRequired();
            builder.Property(f => f.Ruta).HasMaxLength(260);
            builder.Property(f => f.Descripcion).IsRequired().HasMaxLength(120);
            builder.Property(f => f.SubidaPor).IsRequired().HasMaxLength(80);

            // Si se borrara físicamente el vehículo, sus fotografías se borran con él.
            builder.HasOne(f => f.Vehiculo)
                   .WithMany(v => v.Fotografias)
                   .HasForeignKey(f => f.VehiculoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
