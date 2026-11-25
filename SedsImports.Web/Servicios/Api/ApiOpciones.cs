using System.ComponentModel.DataAnnotations;

namespace SedsImports.Web.Servicios.Api
{
    /// <summary>Dirección de la API REST que consume la aplicación web (sección "Api" de appsettings).</summary>
    public class ApiOpciones
    {
        public const string Seccion = "Api";

        [Required, Url]
        public string UrlBase { get; set; } = string.Empty;

        [Range(5, 120)]
        public int SegundosEspera { get; set; } = 20;
    }
}
