namespace SedsImports.Web.Servicios.Api
{
    /// <summary>
    /// Resultado de una llamada a la API: el dato si todo salió bien, o el código HTTP y el
    /// mensaje de error (tomado del ProblemDetails que devuelve la API).
    /// </summary>
    public class RespuestaApi<T>
    {
        public bool Exitosa { get; init; }
        public int Codigo { get; init; }
        public T? Dato { get; init; }
        public string? Error { get; init; }
        public Dictionary<string, string[]> ErroresCampo { get; init; } = new();

        public static RespuestaApi<T> Ok(T dato, int codigo = 200) => new() { Exitosa = true, Codigo = codigo, Dato = dato };

        public static RespuestaApi<T> Falla(int codigo, string error, Dictionary<string, string[]>? errores = null) =>
            new() { Exitosa = false, Codigo = codigo, Error = error, ErroresCampo = errores ?? new() };
    }
}
