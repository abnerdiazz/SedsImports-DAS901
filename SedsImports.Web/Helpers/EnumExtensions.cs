using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SedsImports.Web.Models;

namespace SedsImports.Web.Helpers
{
    /// <summary>Métodos de ayuda para mostrar los enums con nombres legibles y sus colores.</summary>
    public static class EnumExtensions
    {
        /// <summary>Devuelve el texto del atributo [Display(Name = ...)] o el nombre del valor.</summary>
        public static string NombreVisible(this Enum valor)
        {
            var miembro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
            var display = miembro?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? valor.ToString();
        }

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
            _ => "punto-gris"
        };

        /// <summary>Nombre corto del rol para tablas (Patio, Mecánico, Asistente, Vendedor).</summary>
        public static string NombreCorto(this TipoRol rol) => rol switch
        {
            TipoRol.EncargadoPatio => "Patio",
            TipoRol.Mecanico => "Mecánico",
            TipoRol.AsistenteAdministrativa => "Asistente",
            TipoRol.DuenoVendedor => "Vendedor",
            _ => rol.ToString()
        };
    }
}
