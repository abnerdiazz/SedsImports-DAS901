using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>Rol que determina qué operaciones puede realizar un usuario (tabla Roles).</summary>
    public class Rol : IEntidad
    {
        public int Id { get; set; }

        /// <summary>Tipo de rol. Es único: hay un solo registro por cada perfil.</summary>
        public TipoRol Tipo { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Relación 1 a N: un rol tiene muchos usuarios.</summary>
        public List<Usuario> Usuarios { get; set; } = new();
    }
}
