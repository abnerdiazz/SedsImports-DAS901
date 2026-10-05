namespace SedsImports.Web.Services
{
    /// <summary>
    /// Resultado de una operación de negocio. Los servicios no lanzan excepciones por reglas
    /// del negocio: devuelven este objeto y el controlador decide qué mostrar.
    /// </summary>
    public class ResultadoOperacion
    {
        public bool Exitoso { get; protected set; }
        public string Mensaje { get; protected set; } = string.Empty;

        /// <summary>Errores asociados a un campo del formulario (se pasan al ModelState).</summary>
        public Dictionary<string, string> ErroresCampo { get; } = new();

        public static ResultadoOperacion Ok(string mensaje) => new() { Exitoso = true, Mensaje = mensaje };

        public static ResultadoOperacion Error(string mensaje) => new() { Exitoso = false, Mensaje = mensaje };

        public static ResultadoOperacion ErrorCampo(string campo, string mensaje)
        {
            var r = new ResultadoOperacion { Exitoso = false, Mensaje = mensaje };
            r.ErroresCampo[campo] = mensaje;
            return r;
        }
    }

    /// <summary>Resultado que además devuelve un dato (por ejemplo, el vehículo recién registrado).</summary>
    public class ResultadoOperacion<T> : ResultadoOperacion
    {
        public T? Dato { get; private set; }

        public static ResultadoOperacion<T> Ok(T dato, string mensaje) => new() { Exitoso = true, Mensaje = mensaje, Dato = dato };

        public static new ResultadoOperacion<T> Error(string mensaje) => new() { Exitoso = false, Mensaje = mensaje };

        public static new ResultadoOperacion<T> ErrorCampo(string campo, string mensaje)
        {
            var r = new ResultadoOperacion<T> { Exitoso = false, Mensaje = mensaje };
            r.ErroresCampo[campo] = mensaje;
            return r;
        }

        public static ResultadoOperacion<T> Desde(ResultadoOperacion otro)
        {
            var r = new ResultadoOperacion<T> { Exitoso = otro.Exitoso, Mensaje = otro.Mensaje };
            foreach (var e in otro.ErroresCampo) r.ErroresCampo[e.Key] = e.Value;
            return r;
        }
    }
}
