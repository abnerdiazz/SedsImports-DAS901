using SedsImports.Web.Models;

namespace SedsImports.Web.Repositories
{
    /// <summary>
    /// Repositorio genérico en memoria. Se registra como Singleton para que los datos
    /// se conserven mientras la aplicación esté en ejecución (se reinician al detenerla).
    /// Usa un candado (lock) porque varias peticiones pueden llegar al mismo tiempo.
    /// </summary>
    public class RepositorioMemoria<T> : IRepositorio<T> where T : class, IEntidad
    {
        protected readonly List<T> Datos = new();
        protected readonly object Candado = new();
        private int _ultimoId;

        public virtual IReadOnlyList<T> ObtenerTodos()
        {
            lock (Candado) return Datos.ToList();
        }

        public virtual T? ObtenerPorId(int id)
        {
            lock (Candado) return Datos.FirstOrDefault(e => e.Id == id);
        }

        public T Agregar(T entidad)
        {
            lock (Candado)
            {
                if (entidad.Id == 0) entidad.Id = ++_ultimoId;
                else _ultimoId = Math.Max(_ultimoId, entidad.Id);
                Datos.Add(entidad);
                return entidad;
            }
        }

        public void Actualizar(T entidad)
        {
            lock (Candado)
            {
                var indice = Datos.FindIndex(e => e.Id == entidad.Id);
                if (indice >= 0) Datos[indice] = entidad;
            }
        }

        public virtual bool Eliminar(int id)
        {
            lock (Candado) return Datos.RemoveAll(e => e.Id == id) > 0;
        }

        protected IReadOnlyList<T> Buscar(Func<T, bool> condicion)
        {
            lock (Candado) return Datos.Where(condicion).ToList();
        }

        protected T? BuscarUno(Func<T, bool> condicion)
        {
            lock (Candado) return Datos.FirstOrDefault(condicion);
        }
    }

    /// <summary>
    /// Vehículos. La eliminación es lógica: los vehículos marcados como eliminados
    /// no se devuelven en las consultas normales, pero se conservan en los datos.
    /// </summary>
    public class VehiculoRepositoryMemoria : RepositorioMemoria<Vehiculo>, IVehiculoRepository
    {
        private int _ultimaFoto;

        public override IReadOnlyList<Vehiculo> ObtenerTodos() => Buscar(v => !v.Eliminado);

        public override Vehiculo? ObtenerPorId(int id) => BuscarUno(v => v.Id == id && !v.Eliminado);

        public Vehiculo? ObtenerIncluyendoEliminados(int id) => BuscarUno(v => v.Id == id);

        public override bool Eliminar(int id)
        {
            lock (Candado)
            {
                var vehiculo = Datos.FirstOrDefault(v => v.Id == id && !v.Eliminado);
                if (vehiculo == null) return false;
                vehiculo.Eliminado = true;
                vehiculo.FechaEliminacion ??= DateTime.Now;
                return true;
            }
        }

        public Vehiculo? ObtenerPorVin(string vin) =>
            BuscarUno(v => !v.Eliminado && string.Equals(v.Vin, vin.Trim(), StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<Vehiculo> ObtenerPorEstado(params EstadoVehiculo[] estados) =>
            Buscar(v => !v.Eliminado && estados.Contains(v.Estado));

        public int SiguienteIdFoto() => Interlocked.Increment(ref _ultimaFoto);
    }

    public class RevisionRepositoryMemoria : RepositorioMemoria<Revision>, IRevisionRepository
    {
        private int _ultimaNota;

        public Revision? ObtenerPorVehiculo(int vehiculoId) => BuscarUno(r => r.VehiculoId == vehiculoId);

        public int SiguienteIdNota() => Interlocked.Increment(ref _ultimaNota);
    }

    public class DocumentacionRepositoryMemoria : RepositorioMemoria<Documentacion>, IDocumentacionRepository
    {
        public Documentacion? ObtenerPorVehiculo(int vehiculoId) => BuscarUno(d => d.VehiculoId == vehiculoId);
    }

    public class ClienteRepositoryMemoria : RepositorioMemoria<Cliente>, IClienteRepository
    {
        public Cliente? ObtenerPorNombreYTelefono(string nombre, string? telefono) =>
            BuscarUno(c => string.Equals(c.Nombre, nombre.Trim(), StringComparison.OrdinalIgnoreCase)
                           && (telefono == null || c.Telefono == telefono));
    }

    public class ApartadoRepositoryMemoria : RepositorioMemoria<Apartado>, IApartadoRepository
    {
        public Apartado? ObtenerActivoPorVehiculo(int vehiculoId) =>
            BuscarUno(a => a.VehiculoId == vehiculoId && a.Estado == EstadoApartado.Activo);

        public IReadOnlyList<Apartado> ObtenerActivos() => Buscar(a => a.Estado == EstadoApartado.Activo);
    }

    public class VentaRepositoryMemoria : RepositorioMemoria<Venta>, IVentaRepository
    {
        public Venta? ObtenerPorVehiculo(int vehiculoId) => BuscarUno(v => v.VehiculoId == vehiculoId);
    }

    public class RolRepositoryMemoria : RepositorioMemoria<Rol>, IRolRepository
    {
        public Rol? ObtenerPorTipo(TipoRol tipo) => BuscarUno(r => r.Tipo == tipo);
    }

    public class UsuarioRepositoryMemoria : RepositorioMemoria<Usuario>, IUsuarioRepository
    {
        public Usuario? ObtenerPorCorreo(string correo) =>
            BuscarUno(u => string.Equals(u.Correo, correo.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public class BitacoraRepositoryMemoria : RepositorioMemoria<MovimientoBitacora>, IBitacoraRepository
    {
        public IReadOnlyList<MovimientoBitacora> ObtenerPorVehiculo(int vehiculoId) =>
            Buscar(m => m.VehiculoId == vehiculoId).OrderBy(m => m.Fecha).ToList();
    }
}
