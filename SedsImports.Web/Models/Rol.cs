namespace SedsImports.Web.Models
{
    /// <summary>Rol que determina qué operaciones puede realizar un usuario.</summary>
    public class Rol : IEntidad
    {
        public int Id { get; set; }
        public TipoRol Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public List<Usuario> Usuarios { get; set; } = new();
    }

    /// <summary>Contrato mínimo de toda entidad guardada en un repositorio.</summary>
    public interface IEntidad
    {
        int Id { get; set; }
    }
}
