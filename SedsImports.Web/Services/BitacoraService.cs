using SedsImports.Web.Models;
using SedsImports.Web.Repositories;

namespace SedsImports.Web.Services
{
    /// <summary>
    /// Bitácora de cambios: cada servicio registra aquí lo que hizo un usuario.
    /// Permite la trazabilidad completa de cada vehículo ("quién hizo cada cambio").
    /// </summary>
    public class BitacoraService : IBitacoraService
    {
        private readonly IBitacoraRepository _bitacora;

        public BitacoraService(IBitacoraRepository bitacora) => _bitacora = bitacora;

        public void Registrar(Usuario actor, Vehiculo? vehiculo, TipoMovimiento tipo, string accion)
        {
            _bitacora.Agregar(new MovimientoBitacora
            {
                Fecha = DateTime.Now,
                UsuarioId = actor.Id,
                UsuarioNombre = actor.NombreCompleto,
                RolNombre = actor.Rol.Nombre,
                RolTipo = actor.Rol.Tipo,
                VehiculoId = vehiculo?.Id,
                VehiculoNombre = vehiculo?.NombreCompleto,
                Tipo = tipo,
                Accion = accion
            });
        }

        public List<MovimientoBitacora> Listar(string? busqueda)
        {
            var consulta = _bitacora.ObtenerTodos().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(m =>
                    m.UsuarioNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    m.RolNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    (m.VehiculoNombre ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    m.Accion.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }
            return consulta.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id).ToList();
        }

        public List<MovimientoBitacora> PorVehiculo(int vehiculoId) =>
            _bitacora.ObtenerPorVehiculo(vehiculoId).OrderBy(m => m.Fecha).ThenBy(m => m.Id).ToList();

        public List<MovimientoBitacora> Recientes(int cantidad) =>
            _bitacora.ObtenerTodos().OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id).Take(cantidad).ToList();
    }
}
