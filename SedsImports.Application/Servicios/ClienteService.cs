using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>Datos básicos del cliente para apartados y ventas (RF-08).</summary>
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clientes;

        public ClienteService(IClienteRepository clientes) => _clientes = clientes;

        public Cliente? Obtener(int id) => _clientes.ObtenerPorId(id);

        /// <summary>
        /// Reutiliza el cliente si ya existe con el mismo nombre y teléfono; si no, lo agrega.
        /// No guarda: el apartado o la venta que lo usa confirma todo junto con la Unidad de Trabajo.
        /// </summary>
        public Cliente ObtenerOCrear(string nombre, string? telefono)
        {
            var nombreLimpio = nombre.Trim();
            var telefonoLimpio = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

            var existente = _clientes.ObtenerPorNombreYTelefono(nombreLimpio, telefonoLimpio);
            if (existente != null) return existente;

            return _clientes.Agregar(new Cliente { Nombre = nombreLimpio, Telefono = telefonoLimpio });
        }
    }
}
