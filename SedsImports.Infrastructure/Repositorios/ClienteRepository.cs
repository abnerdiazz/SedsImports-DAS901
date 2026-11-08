using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    public class ClienteRepository : RepositorioBase<Cliente>, IClienteRepository
    {
        public ClienteRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        public Cliente? ObtenerPorNombreYTelefono(string nombre, string? telefono)
        {
            var limpio = nombre.Trim();
            return Tabla.FirstOrDefault(c => c.Nombre == limpio && (telefono == null || c.Telefono == telefono));
        }
    }
}
