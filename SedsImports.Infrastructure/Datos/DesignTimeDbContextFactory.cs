using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SedsImports.Infrastructure.Datos
{
    /// <summary>
    /// Lo usa la herramienta dotnet-ef (Add-Migration / Update-Database) para crear el contexto
    /// en tiempo de diseño, sin tener que arrancar la aplicación web.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SedsImportsDbContext>
    {
        public const string CadenaLocalDb =
            "Server=(localdb)\\MSSQLLocalDB;Database=SedsImportsDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public SedsImportsDbContext CreateDbContext(string[] args)
        {
            var opciones = new DbContextOptionsBuilder<SedsImportsDbContext>()
                .UseSqlServer(CadenaLocalDb)
                .Options;
            return new SedsImportsDbContext(opciones);
        }
    }
}
