using Microsoft.EntityFrameworkCore;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>
    /// Repositorio genérico con Entity Framework Core. Implementa el CRUD común a todas las tablas.
    /// Los repositorios de cada entidad heredan de esta clase y agregan sus consultas propias
    /// (principio abierto/cerrado: se extiende sin modificar la base).
    /// Ninguna operación guarda por sí sola: los cambios se confirman con la Unidad de Trabajo.
    /// </summary>
    public class RepositorioBase<T> : IRepositorio<T> where T : class, IEntidad
    {
        protected readonly SedsImportsDbContext Contexto;
        protected readonly DbSet<T> Tabla;

        public RepositorioBase(SedsImportsDbContext contexto)
        {
            Contexto = contexto;
            Tabla = contexto.Set<T>();
        }

        /// <summary>Consulta base de la entidad. Las clases hijas la sobrescriben para incluir sus relaciones.</summary>
        protected virtual IQueryable<T> Consulta() => Tabla;

        public virtual IReadOnlyList<T> ObtenerTodos() => Consulta().ToList();

        public virtual T? ObtenerPorId(int id) => Consulta().FirstOrDefault(e => e.Id == id);

        public virtual T Agregar(T entidad)
        {
            Tabla.Add(entidad);
            return entidad;
        }

        public virtual void Actualizar(T entidad)
        {
            // Si la entidad ya viene de una consulta, EF Core detecta los cambios solo;
            // si viene "desconectada", se marca como modificada.
            if (Contexto.Entry(entidad).State == EntityState.Detached)
                Tabla.Update(entidad);
        }

        public virtual void Eliminar(T entidad) => Tabla.Remove(entidad);
    }
}
