using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    public interface IClienteService
    {
        Cliente? Obtener(int id);

        /// <summary>Lista los clientes ordenados por nombre; filtra por nombre o teléfono si se indica.</summary>
        List<Cliente> Listar(string? busqueda);

        /// <summary>Devuelve el cliente existente o uno nuevo (se inserta cuando el servicio que lo usa guarda los cambios).</summary>
        Cliente ObtenerOCrear(string nombre, string? telefono);
    }
}
