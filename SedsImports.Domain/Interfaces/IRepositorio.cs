using SedsImports.Domain.Entidades;

namespace SedsImports.Domain.Interfaces
{
    /// <summary>
    /// Operaciones CRUD comunes a todas las entidades (patrón Repositorio).
    /// Los repositorios NO guardan por su cuenta: los cambios se confirman con la
    /// Unidad de Trabajo (<see cref="IUnidadDeTrabajo"/>) para que varias operaciones
    /// se guarden juntas en una sola transacción.
    /// </summary>
    public interface IRepositorio<T> where T : class, IEntidad
    {
        /// <summary>Consultar: todos los registros.</summary>
        IReadOnlyList<T> ObtenerTodos();

        /// <summary>Consultar: un registro por su llave primaria.</summary>
        T? ObtenerPorId(int id);

        /// <summary>Crear: agrega el registro (se inserta al llamar a GuardarCambios).</summary>
        T Agregar(T entidad);

        /// <summary>Actualizar: marca el registro como modificado.</summary>
        void Actualizar(T entidad);

        /// <summary>Eliminar: borra el registro de la base de datos (eliminación física).</summary>
        void Eliminar(T entidad);
    }
}
