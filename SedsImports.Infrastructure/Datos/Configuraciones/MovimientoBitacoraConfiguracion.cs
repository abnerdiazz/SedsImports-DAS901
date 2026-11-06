using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla MovimientosBitacora: registro de auditoría. Relaciones opcionales con Usuarios y Vehiculos.</summary>
    public class MovimientoBitacoraConfiguracion : IEntityTypeConfiguration<MovimientoBitacora>
    {
        public void Configure(EntityTypeBuilder<MovimientoBitacora> builder)
        {
            builder.ToTable("MovimientosBitacora");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.UsuarioNombre).IsRequired().HasMaxLength(80);
            builder.Property(m => m.RolNombre).IsRequired().HasMaxLength(60);
            builder.Property(m => m.VehiculoNombre).HasMaxLength(80);
            builder.Property(m => m.Tipo).IsRequired();
            builder.Property(m => m.Accion).IsRequired().HasMaxLength(600);

            builder.HasOne(m => m.Usuario)
                   .WithMany()
                   .HasForeignKey(m => m.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);

            // El historial se conserva aunque el vehículo esté eliminado (eliminación lógica).
            builder.HasOne(m => m.Vehiculo)
                   .WithMany()
                   .HasForeignKey(m => m.VehiculoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.Fecha);
            builder.HasIndex(m => new { m.VehiculoId, m.Fecha });
        }
    }
}
