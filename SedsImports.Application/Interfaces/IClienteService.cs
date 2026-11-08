using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    public interface IClienteService
    {
        Cliente? Obtener(int id);

        /// <summary>Devuelve el cliente existente o uno nuevo (se inserta cuando el servicio que lo usa guarda los cambios).</summary>
        Cliente ObtenerOCrear(string nombre, string? telefono);
    }
}
