using SedsImports.Domain.Enums;

namespace SedsImports.Infrastructure.Identidad
{
    /// <summary>
    /// Nombres de los roles de Identity. Coinciden con los valores de TipoRol, así el rol del
    /// token JWT y el rol del negocio son siempre el mismo.
    /// </summary>
    public static class RolesSistema
    {
        public const string EncargadoPatio = nameof(TipoRol.EncargadoPatio);
        public const string Mecanico = nameof(TipoRol.Mecanico);
        public const string AsistenteAdministrativa = nameof(TipoRol.AsistenteAdministrativa);
        public const string DuenoVendedor = nameof(TipoRol.DuenoVendedor);

        public static readonly string[] Todos = { EncargadoPatio, Mecanico, AsistenteAdministrativa, DuenoVendedor };

        public static string De(TipoRol rol) => rol.ToString();
    }
}
