namespace SedsImports.Domain.Excepciones
{
    /// <summary>
    /// Error al guardar en la base de datos (por ejemplo, un dato duplicado o una conexión caída).
    /// La lanza la Unidad de Trabajo y la capturan los servicios para mostrar un mensaje claro
    /// sin depender de Entity Framework.
    /// </summary>
    public class ErrorPersistenciaException : Exception
    {
        public ErrorPersistenciaException(string mensaje, Exception? causa = null) : base(mensaje, causa) { }
    }
}
