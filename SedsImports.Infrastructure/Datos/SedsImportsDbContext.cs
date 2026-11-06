using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SedsImports.Domain.Entidades;

namespace SedsImports.Infrastructure.Datos
{
    /// <summary>
    /// Contexto de Entity Framework Core: representa la sesión con la base de datos SedsImportsDB.
    /// Cada DbSet es una tabla. La configuración de cada tabla (llaves, longitudes, índices y
    /// relaciones) está en la carpeta Configuraciones, una clase por entidad (principio de
    /// responsabilidad única).
    /// </summary>
    public class SedsImportsDbContext : DbContext
    {
        public SedsImportsDbContext(DbContextOptions<SedsImportsDbContext> opciones) : base(opciones)
        {
        }

        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
        public DbSet<Fotografia> Fotografias => Set<Fotografia>();
        public DbSet<Revision> Revisiones => Set<Revision>();
        public DbSet<NotaRevision> NotasRevision => Set<NotaRevision>();
        public DbSet<Documentacion> Documentaciones => Set<Documentacion>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Apartado> Apartados => Set<Apartado>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<MovimientoBitacora> MovimientosBitacora => Set<MovimientoBitacora>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplica todas las clases IEntityTypeConfiguration<T> de este proyecto.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SedsImportsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Todos los montos de dinero se guardan como decimal(12,2).
            configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // El filtro global de vehículos eliminados también oculta su revisión, documentación,
            // fotografías y apartados. Es el comportamiento que queremos, así que se omite la advertencia.
            optionsBuilder.ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        }
    }
}
