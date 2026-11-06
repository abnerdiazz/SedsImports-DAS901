using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Roles: catálogo de perfiles del sistema.</summary>
    public class RolConfiguracion : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Tipo).IsRequired();
            builder.HasIndex(r => r.Tipo).IsUnique();

            builder.Property(r => r.Nombre).IsRequired().HasMaxLength(60);
            builder.Property(r => r.Descripcion).IsRequired().HasMaxLength(200);
        }
    }
}
