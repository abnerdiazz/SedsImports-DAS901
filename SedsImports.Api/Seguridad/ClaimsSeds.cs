namespace SedsImports.Api.Seguridad
{
    /// <summary>Nombres de los datos (claims) propios del sistema que viajan dentro del token JWT.</summary>
    public static class ClaimsSeds
    {
        /// <summary>Id del usuario en la tabla Usuarios.</summary>
        public const string UsuarioId = "uid";

        /// <summary>Nombre completo del usuario, para mostrarlo sin consultar la base de datos.</summary>
        public const string NombreCompleto = "nombre";
    }
}
