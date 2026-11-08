using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>
    /// Bitácora de cambios: cada servicio registra aquí lo que hizo un usuario.
    /// Permite la trazabilidad completa de cada vehículo ("quién hizo cada cambio").
    /// El movimiento se guarda en la misma transacción que la operación que lo originó:
    /// si la operación falla, tampoco queda registrado el movimiento.
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
                Vehiculo = vehiculo,                      // por navegación: sirve también para vehículos recién creados
                VehiculoNombre = vehiculo?.NombreCompleto,
                Tipo = tipo,
                Accion = accion.Length <= 600 ? accion : accion[..600]
            });
        }

        public List<MovimientoBitacora> Listar(string? busqueda) => _bitacora.Buscar(busqueda).ToList();

        public List<MovimientoBitacora> PorVehiculo(int vehiculoId) => _bitacora.ObtenerPorVehiculo(vehiculoId).ToList();

        public List<MovimientoBitacora> Recientes(int cantidad) => _bitacora.Recientes(cantidad).ToList();
    }
}
