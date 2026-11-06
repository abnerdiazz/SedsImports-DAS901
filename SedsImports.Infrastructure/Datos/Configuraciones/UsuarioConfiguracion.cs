using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Usuarios: relación N a 1 con Roles.</summary>
    public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(80);
            builder.Property(u => u.Correo).IsRequired().HasMaxLength(80);
            builder.HasIndex(u => u.Correo).IsUnique();
            builder.Property(u => u.ContrasenaHash).IsRequired().HasMaxLength(64).IsFixedLength();

            // Propiedades calculadas: no son columnas.
            builder.Ignore(u => u.Iniciales);
            builder.Ignore(u => u.PrimerNombre);

            // Un rol tiene muchos usuarios. No se permite borrar un rol que tenga usuarios.
            builder.HasOne(u => u.Rol)
                   .WithMany(r => r.Usuarios)
                   .HasForeignKey(u => u.RolId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
