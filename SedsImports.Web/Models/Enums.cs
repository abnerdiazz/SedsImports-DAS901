using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Models
{
    /// <summary>
    /// Etapas por las que pasa un vehículo desde que llega al patio hasta que se vende.
    /// RevisionFinalizada es la etapa en la que el mecánico ya terminó, pero el dueño
    /// todavía no autoriza que el vehículo quede disponible para la venta.
    /// </summary>
    public enum EstadoVehiculo
    {
        [Display(Name = "Recién llegado")]
        RecienLlegado = 1,

        [Display(Name = "En revisión / preparación")]
        EnRevision = 2,

        [Display(Name = "Pendiente de autorización")]
        RevisionFinalizada = 3,

        [Display(Name = "Disponible para venta")]
        Disponible = 4,

        [Display(Name = "Apartado")]
        Apartado = 5,

        [Display(Name = "Vendido")]
        Vendido = 6
    }

    /// <summary>Proceso paralelo: no detiene la revisión ni la venta.</summary>
    public enum EstadoDocumentacion
    {
        [Display(Name = "Pendiente")]
        Pendiente = 1,

        [Display(Name = "Completa")]
        Completa = 2
    }

    /// <summary>Perfiles internos de SED's Imports &amp; Services.</summary>
    public enum TipoRol
    {
        [Display(Name = "Encargado de patio")]
        EncargadoPatio = 1,

        [Display(Name = "Mecánico")]
        Mecanico = 2,

        [Display(Name = "Asistente administrativa")]
        AsistenteAdministrativa = 3,

        [Display(Name = "Dueño / vendedor")]
        DuenoVendedor = 4
    }

    /// <summary>Momento del proceso en el que se tomó una fotografía.</summary>
    public enum TipoFoto
    {
        [Display(Name = "Al ingreso")]
        Ingreso = 1,

        [Display(Name = "Durante la revisión")]
        Revision = 2,

        [Display(Name = "Fotografías finales")]
        Final = 3
    }

    public enum EstadoApartado
    {
        [Display(Name = "Activo")]
        Activo = 1,

        [Display(Name = "Convertido en venta")]
        ConvertidoEnVenta = 2,

        [Display(Name = "Cancelado")]
        Cancelado = 3
    }

    /// <summary>Clasifica los movimientos de la bitácora (define el color del punto en la trazabilidad).</summary>
    public enum TipoMovimiento
    {
        [Display(Name = "Registro")]
        Registro = 1,

        [Display(Name = "Edición")]
        Edicion = 2,

        [Display(Name = "Revisión")]
        Revision = 3,

        [Display(Name = "Documentación")]
        Documentacion = 4,

        [Display(Name = "Autorización")]
        Autorizacion = 5,

        [Display(Name = "Comercial")]
        Comercial = 6,

        [Display(Name = "Usuarios")]
        Usuario = 7
    }
}
