using SedsImports.Domain.Consultas;
using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    public interface IVentaRepository : IRepositorio<Venta>
    {
        /// <summary>Venta de un vehículo, con su cliente.</summary>
        Venta? ObtenerPorVehiculo(int vehiculoId);

        /// <summary>Ventas con su vehículo y su cliente (join). Si se indica el año, solo las de ese año.</summary>
        IReadOnlyList<Venta> ObtenerConDetalle(int? anio = null);

        /// <summary>Unidades y total vendido por mes de un año (agrupación).</summary>
        IReadOnlyList<ResumenVentasMes> ResumenMensual(int anio);

        /// <summary>Unidades, total y días promedio en el patio por marca (consulta avanzada).</summary>
        IReadOnlyList<ResumenVentasMarca> ResumenPorMarca(int anio);

        /// <summary>Años que tienen ventas registradas.</summary>
        IReadOnlyList<int> AniosConVentas();

        /// <summary>Unidades y total vendido en un mes.</summary>
        (int Cantidad, decimal Total) TotalesDelMes(int anio, int mes);
    }
}
