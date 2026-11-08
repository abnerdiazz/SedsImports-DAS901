using SedsImports.Domain.Enums;

namespace SedsImports.Web.Helpers
{
    /// <summary>Clases CSS según el estado: es un detalle de presentación, por eso vive en la capa Web.</summary>
    public static class EstilosExtensions
    {
        /// <summary>Clase CSS de la insignia según el estado del vehículo.</summary>
        public static string ClaseCss(this EstadoVehiculo estado) => estado switch
        {
            EstadoVehiculo.RecienLlegado => "badge-recien",
            EstadoVehiculo.EnRevision => "badge-revision",
            EstadoVehiculo.RevisionFinalizada => "badge-autorizacion",
            EstadoVehiculo.Disponible => "badge-disponible",
            EstadoVehiculo.Apartado => "badge-apartado",
            EstadoVehiculo.Vendido => "badge-vendido",
            _ => "badge-neutro"
        };

        public static string ClaseCss(this EstadoDocumentacion estado) =>
            estado == EstadoDocumentacion.Completa ? "badge-disponible" : "badge-revision";

        /// <summary>Clase CSS del punto de color en la trazabilidad, según quién hizo el cambio.</summary>
        public static string ClaseCss(this TipoMovimiento tipo) => tipo switch
        {
            TipoMovimiento.Registro => "punto-azul",
            TipoMovimiento.Edicion => "punto-gris",
            TipoMovimiento.Revision => "punto-naranja",
            TipoMovimiento.Documentacion => "punto-verde",
            TipoMovimiento.Autorizacion => "punto-amarillo",
            TipoMovimiento.Comercial => "punto-morado",
            TipoMovimiento.Eliminacion => "punto-rojo",
            _ => "punto-gris"
        };

    }
}
