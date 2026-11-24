using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SedsImports.Infrastructure.Identidad
{
    /// <summary>
    /// Contexto de ASP.NET Core Identity. Usa la misma base de datos SedsImportsDB, pero sus tablas
    /// (AspNetUsers, AspNetRoles, AspNetUserRoles...) quedan en el esquema "seguridad" y tienen su
    /// propio historial de migraciones. Así la seguridad queda separada de los datos del negocio.
    /// </summary>
    public class IdentidadDbContext : IdentityDbContext<UsuarioIdentidad, IdentityRole<int>, int>
    {
        public const string Esquema = "seguridad";
        public const string TablaHistorial = "__MigracionesIdentidad";

        public IdentidadDbContext(DbContextOptions<IdentidadDbContext> opciones) : base(opciones)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema(Esquema);

            builder.Entity<UsuarioIdentidad>(u =>
            {
                u.Property(x => x.NombreCompleto).HasMaxLength(80).IsRequired();
                // Cada usuario del negocio tiene una sola cuenta de acceso.
                u.HasIndex(x => x.UsuarioId).IsUnique();
            });
        }
    }
}
