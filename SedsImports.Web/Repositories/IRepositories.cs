using SedsImports.Web.Models;

namespace SedsImports.Web.Repositories
{
    /// <summary>
    /// Operaciones básicas de acceso a datos. En la Fase 2 se implementan en memoria;
    /// en la Fase 3 se reemplazan por repositorios con Entity Framework Core
    /// sin tocar servicios ni controladores (principio de inversión de dependencias).
    /// </summary>
    public interface IRepositorio<T> where T : class, IEntidad
    {
        IReadOnlyList<T> ObtenerTodos();
        T? ObtenerPorId(int id);
        T Agregar(T entidad);
        void Actualizar(T entidad);
        bool Eliminar(int id);
    }

    public interface IVehiculoRepository : IRepositorio<Vehiculo>
    {
        Vehiculo? ObtenerPorVin(string vin);
        IReadOnlyList<Vehiculo> ObtenerPorEstado(params EstadoVehiculo[] estados);
        int SiguienteIdFoto();
    }

    public interface IRevisionRepository : IRepositorio<Revision>
    {
        Revision? ObtenerPorVehiculo(int vehiculoId);
        int SiguienteIdNota();
    }

    public interface IDocumentacionRepository : IRepositorio<Documentacion>
    {
        Documentacion? ObtenerPorVehiculo(int vehiculoId);
    }

    public interface IClienteRepository : IRepositorio<Cliente>
    {
        Cliente? ObtenerPorNombreYTelefono(string nombre, string? telefono);
    }

    public interface IApartadoRepository : IRepositorio<Apartado>
    {
        Apartado? ObtenerActivoPorVehiculo(int vehiculoId);
        IReadOnlyList<Apartado> ObtenerActivos();
    }

    public interface IVentaRepository : IRepositorio<Venta>
    {
        Venta? ObtenerPorVehiculo(int vehiculoId);
    }

    public interface IRolRepository : IRepositorio<Rol>
    {
        Rol? ObtenerPorTipo(TipoRol tipo);
    }

    public interface IUsuarioRepository : IRepositorio<Usuario>
    {
        Usuario? ObtenerPorCorreo(string correo);
    }

    public interface IBitacoraRepository : IRepositorio<MovimientoBitacora>
    {
        IReadOnlyList<MovimientoBitacora> ObtenerPorVehiculo(int vehiculoId);
    }
}
