using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos.Configuraciones
{
    /// <summary>Tabla Vehiculos: entidad central. Incluye la eliminación lógica.</summary>
    public class VehiculoConfiguracion : IEntityTypeConfiguration<Vehiculo>
    {
        public void Configure(EntityTypeBuilder<Vehiculo> builder)
        {
            builder.ToTable("Vehiculos");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Vin).IsRequired().HasMaxLength(17);
            builder.Property(v => v.Marca).IsRequired().HasMaxLength(40);
            builder.Property(v => v.Modelo).IsRequired().HasMaxLength(40);
            builder.Property(v => v.Color).IsRequired().HasMaxLength(30);
            builder.Property(v => v.Detalles).HasMaxLength(500);
            builder.Property(v => v.Estado).IsRequired();
            builder.Property(v => v.RegistradoPorNombre).IsRequired().HasMaxLength(80);
            builder.Property(v => v.EditadoPorNombre).HasMaxLength(80);
            builder.Property(v => v.EliminadoPorNombre).HasMaxLength(80);

            // El VIN no se repite entre los vehículos activos (índice único filtrado):
            // un vehículo eliminado no impide registrar de nuevo ese VIN.
            builder.HasIndex(v => v.Vin).IsUnique().HasFilter("[Eliminado] = 0");

            // Índices para las búsquedas y filtros más frecuentes.
            builder.HasIndex(v => v.Estado);
            builder.HasIndex(v => v.FechaIngreso);

            // Llave foránea hacia el usuario que registró el vehículo (sin propiedad de navegación).
            builder.HasOne<Usuario>()
                   .WithMany()
                   .HasForeignKey(v => v.RegistradoPorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Filtro global: las consultas normales nunca devuelven vehículos eliminados.
            builder.HasQueryFilter(v => !v.Eliminado);

            builder.Ignore(v => v.NombreCompleto);
            builder.Ignore(v => v.VinCorto);
        }
    }
}
