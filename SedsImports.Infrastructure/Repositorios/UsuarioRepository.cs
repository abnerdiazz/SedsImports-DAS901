using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>Usuarios. Todas las consultas traen el rol (lo necesitan los permisos y la bitácora).</summary>
    public class UsuarioRepository : RepositorioBase<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(SedsImportsDbContext contexto) : base(contexto)
        {
        }

        protected override IQueryable<Usuario> Consulta() => Tabla.Include(u => u.Rol);

        public Usuario? ObtenerPorCorreo(string correo)
        {
            var limpio = correo.Trim().ToLower();
            return Consulta().FirstOrDefault(u => u.Correo == limpio);
        }
    }
}
