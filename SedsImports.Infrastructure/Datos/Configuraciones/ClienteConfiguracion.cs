using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Clientes.</summary>
    public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nombre).IsRequired().HasMaxLength(80);
            builder.Property(c => c.Telefono).HasMaxLength(20);
            builder.Property(c => c.Correo).HasMaxLength(80);

            // Para encontrar rápido un cliente que ya existe (nombre + teléfono).
            builder.HasIndex(c => new { c.Nombre, c.Telefono });
        }
    }
}
