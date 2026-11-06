using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla NotasRevision: relación N a 1 con Revisiones y con Usuarios.</summary>
    public class NotaRevisionConfiguracion : IEntityTypeConfiguration<NotaRevision>
    {
        public void Configure(EntityTypeBuilder<NotaRevision> builder)
        {
            builder.ToTable("NotasRevision");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.UsuarioNombre).IsRequired().HasMaxLength(80);
            builder.Property(n => n.Texto).IsRequired().HasMaxLength(500);

            builder.HasOne(n => n.Revision)
                   .WithMany(r => r.Notas)
                   .HasForeignKey(n => n.RevisionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Usuario>()
                   .WithMany()
                   .HasForeignKey(n => n.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Para la consulta "últimas notas de un usuario".
            builder.HasIndex(n => new { n.UsuarioId, n.Fecha });
        }
    }
}
