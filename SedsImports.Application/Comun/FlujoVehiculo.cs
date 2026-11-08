using SedsImports.Domain.Enums;

namespace SedsImports.Application.Comun
{
    /// <summary>Etapas del proceso del vehículo: qué sigue y quién debe hacerlo.</summary>
    public static class FlujoVehiculo
    {
        public static (string? Accion, string? Responsable) SiguienteAccion(EstadoVehiculo estado) => estado switch
        {
            EstadoVehiculo.RecienLlegado => ("Iniciar revisión", "Mecánico"),
            EstadoVehiculo.EnRevision => ("Finalizar preparación", "Mecánico"),
            EstadoVehiculo.RevisionFinalizada => ("Autorizar disponibilidad", "Dueño"),
            EstadoVehiculo.Apartado => ("Registrar venta", "Dueño"),
            _ => (null, null)
        };
    }
}
