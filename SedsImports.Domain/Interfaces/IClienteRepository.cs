using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IClienteRepository : IRepositorio<Cliente>
    {
        /// <summary>Cliente con el mismo nombre y teléfono (para no duplicar clientes).</summary>
        Cliente? ObtenerPorNombreYTelefono(string nombre, string? telefono);
    }
}
