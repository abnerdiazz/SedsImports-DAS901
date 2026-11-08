using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IBitacoraService
    {
        /// <summary>Agrega el movimiento. Se guarda junto con la operación que lo originó (misma Unidad de Trabajo).</summary>
        void Registrar(Usuario actor, Vehiculo? vehiculo, TipoMovimiento tipo, string accion);
        List<MovimientoBitacora> Listar(string? busqueda);
        List<MovimientoBitacora> PorVehiculo(int vehiculoId);
        List<MovimientoBitacora> Recientes(int cantidad);
    }
}
