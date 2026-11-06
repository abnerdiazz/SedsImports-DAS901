namespace SedsImports.Domain.Entidades
{
    /// <summary>
    /// Contrato mínimo de toda entidad que se guarda en la base de datos: una llave primaria entera.
    /// Permite que el repositorio genérico trabaje con cualquier entidad (principio de sustitución de Liskov).
    /// </summary>
    public interface IEntidad
    {
        int Id { get; set; }
    }
}
