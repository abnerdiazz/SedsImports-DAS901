using SedsImports.Web.DTOs;
using SedsImports.Web.Models;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    // Interfaces separadas por responsabilidad (principio de segregación de interfaces).
    // Los controladores dependen de estas abstracciones, no de las clases concretas.

    public interface IVehiculoService
    {
        List<VehiculoListadoDto> Listar(string? busqueda = null, EstadoVehiculo? estado = null, IEnumerable<EstadoVehiculo>? estadosVisibles = null);
        ConteoEstadosDto Conteos(IEnumerable<EstadoVehiculo>? estadosVisibles = null);
        Vehiculo? Obtener(int id);
        VehiculoListadoDto ADto(Vehiculo vehiculo);
        int RegistradosDesde(DateTime desde);
        Task<ResultadoOperacion<Vehiculo>> RegistrarAsync(VehiculoFormViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> EditarAsync(VehiculoFormViewModel modelo, Usuario actor);
        ResultadoOperacion AutorizarDisponibilidad(int vehiculoId, Usuario actor);
    }

    public interface IFotoService
    {
        Task<ResultadoOperacion<int>> GuardarAsync(Vehiculo vehiculo, IEnumerable<IFormFile>? archivos, TipoFoto tipo, Usuario actor);
    }

    public interface IRevisionService
    {
        Revision? ObtenerPorVehiculo(int vehiculoId);
        ResultadoOperacion Iniciar(int vehiculoId, Usuario actor);
        ResultadoOperacion AgregarNota(NotaRevisionViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> FinalizarAsync(FinalizarRevisionViewModel modelo, Usuario actor);
        ResultadoOperacion RegresarARevision(RegresarRevisionViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> SubirFotosAsync(SubirFotosViewModel modelo, Usuario actor);
        List<NotaResumen> NotasRecientes(int usuarioId, int cantidad);
    }

    public interface IDocumentacionService
    {
        Documentacion ObtenerPorVehiculo(int vehiculoId);
        DocumentacionIndexViewModel Listar(string? busqueda, EstadoDocumentacion? filtro);
        DocumentacionViewModel ArmarFormulario(int vehiculoId);
        ResultadoOperacion Actualizar(DocumentacionViewModel modelo, Usuario actor);
        (int Pendientes, int Completas) Conteos();
    }

    public interface IClienteService
    {
        Cliente? Obtener(int id);
        Cliente ObtenerOCrear(string nombre, string? telefono);
    }

    public interface IApartadoService
    {
        List<ApartadoListadoDto> ListarActivos();
        Apartado? ObtenerActivo(int vehiculoId);
        ResultadoOperacion Registrar(ApartadoViewModel modelo, Usuario actor);
        ResultadoOperacion Cancelar(CancelarApartadoViewModel modelo, Usuario actor);
    }

    public interface IVentaService
    {
        List<VentaDetalleDto> Listar();
        Venta? ObtenerPorVehiculo(int vehiculoId);
        ResultadoOperacion Registrar(VentaViewModel modelo, Usuario actor);
        ReporteVentasDto Reporte(int anio, int? mes, int? dia, string? busqueda);
        List<int> AniosConVentas();
        (int Cantidad, decimal Total) VentasDelMes(DateTime fecha);
    }

    public interface IBitacoraService
    {
        void Registrar(Usuario actor, Vehiculo? vehiculo, TipoMovimiento tipo, string accion);
        List<MovimientoBitacora> Listar(string? busqueda);
        List<MovimientoBitacora> PorVehiculo(int vehiculoId);
        List<MovimientoBitacora> Recientes(int cantidad);
    }

    public interface IUsuarioService
    {
        ResultadoOperacion<Usuario> Autenticar(string correo, string contrasena);
        List<Usuario> Listar();
        Usuario? Obtener(int id);
        ResultadoOperacion CambiarEstado(int usuarioId, Usuario actor);
    }

    /// <summary>Usuario que tiene la sesión abierta en la petición actual.</summary>
    public interface IUsuarioActualService
    {
        Usuario? Usuario { get; }
        bool Autenticado { get; }
        TipoRol? Rol { get; }
        void IniciarSesion(Usuario usuario);
        void CerrarSesion();
    }

    /// <summary>
    /// Reglas de "quién puede hacer qué". Se usan en los servicios (validación de negocio)
    /// y en las vistas (mostrar u ocultar botones), para que ambas cosas coincidan siempre.
    /// </summary>
    public static class Permisos
    {
        public static bool PuedeRegistrarVehiculo(TipoRol rol) => rol is TipoRol.EncargadoPatio or TipoRol.DuenoVendedor;
        public static bool PuedeEditarVehiculo(TipoRol rol) => rol is TipoRol.EncargadoPatio or TipoRol.DuenoVendedor;
        public static bool PuedeRevisar(TipoRol rol) => rol == TipoRol.Mecanico;
        public static bool PuedeRegresarARevision(TipoRol rol) => rol is TipoRol.Mecanico or TipoRol.DuenoVendedor;
        public static bool PuedeAutorizar(TipoRol rol) => rol == TipoRol.DuenoVendedor;
        public static bool PuedeDocumentar(TipoRol rol) => rol is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor;
        public static bool PuedeGestionarComercial(TipoRol rol) => rol == TipoRol.DuenoVendedor;
        public static bool PuedeAdministrar(TipoRol rol) => rol == TipoRol.DuenoVendedor;

        /// <summary>El mecánico solo ve los vehículos que le toca trabajar o que ya preparó.</summary>
        public static EstadoVehiculo[] EstadosVisibles(TipoRol rol) => rol == TipoRol.Mecanico
            ? new[] { EstadoVehiculo.RecienLlegado, EstadoVehiculo.EnRevision, EstadoVehiculo.RevisionFinalizada, EstadoVehiculo.Disponible }
            : Enum.GetValues<EstadoVehiculo>();

        public const string SinPermiso = "Tu perfil no tiene permiso para realizar esta acción.";
    }
}
