using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>
    /// Control de la documentación de cada vehículo (RF-07, CU-04).
    /// Es un proceso paralelo: no detiene la revisión ni la venta.
    /// </summary>
    public class DocumentacionService : IDocumentacionService
    {
        private readonly IDocumentacionRepository _documentos;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IVehiculoService _vehiculoService;
        private readonly IBitacoraService _bitacora;

        public DocumentacionService(IDocumentacionRepository documentos, IVehiculoRepository vehiculos,
            IVehiculoService vehiculoService, IBitacoraService bitacora)
        {
            _documentos = documentos;
            _vehiculos = vehiculos;
            _vehiculoService = vehiculoService;
            _bitacora = bitacora;
        }

        public Documentacion ObtenerPorVehiculo(int vehiculoId) =>
            _documentos.ObtenerPorVehiculo(vehiculoId)
            ?? _documentos.Agregar(new Documentacion { VehiculoId = vehiculoId, Observaciones = "Sin documentos registrados." });

        public DocumentacionIndexViewModel Listar(string? busqueda, EstadoDocumentacion? filtro)
        {
            // Los vendidos ya cerraron su expediente: se listan los vehículos en proceso.
            var visibles = Enum.GetValues<EstadoVehiculo>().Where(e => e != EstadoVehiculo.Vendido);
            var vehiculos = _vehiculoService.Listar(busqueda, null, visibles);

            var filas = vehiculos
                .Select(v => new DocumentacionFilaViewModel { Vehiculo = v, Documentacion = ObtenerPorVehiculo(v.Id) })
                .ToList();

            var modelo = new DocumentacionIndexViewModel
            {
                Busqueda = busqueda,
                Filtro = filtro,
                Total = filas.Count,
                Pendientes = filas.Count(f => f.Documentacion.Estado == EstadoDocumentacion.Pendiente),
                Completas = filas.Count(f => f.Documentacion.Estado == EstadoDocumentacion.Completa)
            };

            modelo.Filas = filtro.HasValue
                ? filas.Where(f => f.Documentacion.Estado == filtro.Value).ToList()
                : filas;

            return modelo;
        }

        public DocumentacionViewModel ArmarFormulario(int vehiculoId)
        {
            var vehiculo = _vehiculos.ObtenerPorId(vehiculoId);
            var doc = ObtenerPorVehiculo(vehiculoId);
            return new DocumentacionViewModel
            {
                VehiculoId = vehiculoId,
                Vehiculo = vehiculo?.NombreCompleto ?? string.Empty,
                EstadoOperativo = vehiculo?.Estado ?? EstadoVehiculo.RecienLlegado,
                TieneTitulo = doc.TieneTitulo,
                TieneImportacion = doc.TieneImportacion,
                TieneTarjetaCirculacion = doc.TieneTarjetaCirculacion,
                TieneTraspaso = doc.TieneTraspaso,
                TieneOtros = doc.TieneOtros,
                Estado = doc.Estado,
                Observaciones = doc.Observaciones
            };
        }

        public ResultadoOperacion Actualizar(DocumentacionViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeDocumentar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            // CU-04, flujo alterno: si queda pendiente, se exige indicar el pendiente.
            if (modelo.Estado == EstadoDocumentacion.Pendiente && string.IsNullOrWhiteSpace(modelo.Observaciones))
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Observaciones), "Si la documentación queda pendiente, indica qué documento falta.");

            var completos = modelo.TieneTitulo && modelo.TieneImportacion && modelo.TieneTarjetaCirculacion && modelo.TieneTraspaso;
            if (modelo.Estado == EstadoDocumentacion.Completa && !completos)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Estado), "Para marcarla como completa deben estar los cuatro documentos principales.");

            var doc = ObtenerPorVehiculo(vehiculo.Id);
            var estadoAnterior = doc.Estado;

            doc.TieneTitulo = modelo.TieneTitulo;
            doc.TieneImportacion = modelo.TieneImportacion;
            doc.TieneTarjetaCirculacion = modelo.TieneTarjetaCirculacion;
            doc.TieneTraspaso = modelo.TieneTraspaso;
            doc.TieneOtros = modelo.TieneOtros;
            doc.Estado = modelo.Estado;
            doc.Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones)
                ? (modelo.Estado == EstadoDocumentacion.Completa ? "Expediente completo." : null)
                : modelo.Observaciones.Trim();
            doc.FechaActualizacion = DateTime.Now;
            doc.ActualizadoPor = actor.NombreCompleto;
            _documentos.Actualizar(doc);

            var accion = estadoAnterior == doc.Estado
                ? $"Actualizó documentación ({doc.CantidadDocumentos} documento(s)) · Estado: {doc.Estado.NombreVisible()}"
                : $"Actualizó documentación a {doc.Estado.NombreVisible()}";
            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Documentacion, accion);

            return ResultadoOperacion.Ok($"Documentación de {vehiculo.NombreCompleto} guardada.");
        }

        public (int Pendientes, int Completas) Conteos()
        {
            var activos = _vehiculos.ObtenerTodos().Where(v => v.Estado != EstadoVehiculo.Vendido).Select(v => v.Id).ToHashSet();
            var docs = _documentos.ObtenerTodos().Where(d => activos.Contains(d.VehiculoId)).ToList();
            return (docs.Count(d => d.Estado == EstadoDocumentacion.Pendiente), docs.Count(d => d.Estado == EstadoDocumentacion.Completa));
        }
    }
}
