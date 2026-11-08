using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    public interface IVentaService
    {
        List<VentaDetalleDto> Listar();
        Venta? ObtenerPorVehiculo(int vehiculoId);
        ResultadoOperacion Registrar(VentaViewModel modelo, Usuario actor);
        ReporteVentasDto Reporte(int anio, int? mes, int? dia, string? busqueda);
        List<int> AniosConVentas();
        (int Cantidad, decimal Total) VentasDelMes(DateTime fecha);
    }
}
