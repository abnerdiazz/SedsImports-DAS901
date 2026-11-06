namespace SedsImports.Domain.Consultas
{
    // Resultados de consultas LINQ que no corresponden a una sola tabla.
    // Son objetos de solo lectura (records) que arman los repositorios.

    /// <summary>Anotación de revisión junto con el vehículo al que pertenece.</summary>
    public record NotaConVehiculo(int VehiculoId, string Vehiculo, DateTime Fecha, string Texto);

    /// <summary>Ventas de un mes: unidades y total vendido.</summary>
    public record ResumenVentasMes(int Mes, int Cantidad, decimal Total);

    /// <summary>Ventas de una marca: unidades, total y días promedio desde que llegó al patio hasta que se vendió.</summary>
    public record ResumenVentasMarca(string Marca, int Cantidad, decimal Total, double PromedioDiasEnPatio);
}
