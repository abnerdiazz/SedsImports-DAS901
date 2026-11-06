using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Extensiones
{
    /// <summary>Métodos de ayuda para mostrar los enums con nombres legibles en pantalla y en la bitácora.</summary>
    public static class EnumExtensions
    {
        /// <summary>Devuelve el texto del atributo [Display(Name = ...)] o el nombre del valor.</summary>
        public static string NombreVisible(this Enum valor)
        {
            var miembro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
            var display = miembro?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? valor.ToString();
        }

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
