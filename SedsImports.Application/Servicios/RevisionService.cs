using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Extensiones;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>
    /// Flujo técnico del mecánico (RF-05, CU-02):
    /// Recién llegado → En revisión/preparación → Pendiente de autorización.
    /// Finalizar la revisión NO pone el vehículo disponible: eso lo decide el dueño.
    /// </summary>
    public class RevisionService : ServicioBase, IRevisionService
    {
        private readonly IRevisionRepository _revisiones;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IFotoService _fotos;
        private readonly IBitacoraService _bitacora;

        public RevisionService(IUnidadDeTrabajo unidadDeTrabajo, IRevisionRepository revisiones,
            IVehiculoRepository vehiculos, IFotoService fotos, IBitacoraService bitacora)
            : base(unidadDeTrabajo)
        {
            _revisiones = revisiones;
            _vehiculos = vehiculos;
            _fotos = fotos;
            _bitacora = bitacora;
        }

        public Revision? ObtenerPorVehiculo(int vehiculoId) => _revisiones.ObtenerPorVehiculo(vehiculoId);

        public ResultadoOperacion Iniciar(int vehiculoId, Usuario actor)
        {
            if (!Permisos.PuedeRevisar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(vehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");
            if (vehiculo.Estado != EstadoVehiculo.RecienLlegado)
                return ResultadoOperacion.Error("Solo se puede iniciar la revisión de un vehículo recién llegado.");

            var revision = ObtenerOCrear(vehiculo);
            revision.MecanicoId = actor.Id;
            revision.MecanicoNombre = actor.NombreCompleto;
            revision.FechaInicio = DateTime.Now;
            revision.FechaActualizacion = DateTime.Now;

            vehiculo.Estado = EstadoVehiculo.EnRevision;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, "Inició revisión · Recién llegado → En revisión/preparación");
            return Guardar("Revisión iniciada. Registra cada trabajo en las notas.");
        }

        public ResultadoOperacion AgregarNota(NotaRevisionViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeRevisar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");
            if (vehiculo.Estado != EstadoVehiculo.EnRevision)
                return ResultadoOperacion.Error("Las anotaciones se registran mientras el vehículo está en revisión. Inicia la revisión primero.");

            if (string.IsNullOrWhiteSpace(modelo.Texto))
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Texto), "Escribe la anotación antes de guardarla.");

            var revision = ObtenerOCrear(vehiculo);
            var texto = modelo.Texto.Trim();
            revision.Notas.Add(NuevaNota(actor, texto, esDelSistema: false));
            revision.FechaActualizacion = DateTime.Now;

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, $"Agregó anotación: {texto}");
            return Guardar("Anotación agregada.");
        }

        public async Task<ResultadoOperacion> FinalizarAsync(FinalizarRevisionViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeRevisar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");
            if (vehiculo.Estado != EstadoVehiculo.EnRevision)
                return ResultadoOperacion.Error("Solo se puede finalizar un vehículo que está en revisión.");

            if (string.IsNullOrWhiteSpace(modelo.ResumenFinal))
                return ResultadoOperacion.ErrorCampo(nameof(modelo.ResumenFinal), "Describe brevemente los trabajos realizados.");

            var fotos = await _fotos.GuardarAsync(vehiculo, modelo.FotosFinalesNuevas, TipoFoto.Final, actor);
            if (!fotos.Exitoso) return ResultadoOperacion.ErrorCampo(nameof(modelo.FotosFinalesNuevas), fotos.Mensaje);

            var revision = ObtenerOCrear(vehiculo);
            var pendientes = string.IsNullOrWhiteSpace(modelo.Pendientes) ? "Ninguno." : modelo.Pendientes.Trim();

            revision.ResumenFinal = modelo.ResumenFinal.Trim();
            revision.Pendientes = pendientes;
            revision.FechaFinalizacion = DateTime.Now;
            revision.FechaAutorizacion = null;
            revision.AutorizadoPor = null;
            revision.FechaActualizacion = DateTime.Now;
            revision.Notas.Add(NuevaNota(actor, $"Revisión finalizada. Trabajos: {revision.ResumenFinal} Pendientes: {pendientes}", esDelSistema: true));

            vehiculo.Estado = EstadoVehiculo.RevisionFinalizada;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, "Finalizó revisión · En revisión → Pendiente de autorización");
            return Guardar("Revisión finalizada. El vehículo queda pendiente de que el dueño autorice su disponibilidad para la venta.");
        }

        public ResultadoOperacion RegresarARevision(RegresarRevisionViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeRegresarARevision(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");
            if (vehiculo.Estado is not (EstadoVehiculo.RevisionFinalizada or EstadoVehiculo.Disponible))
                return ResultadoOperacion.Error("Solo se puede regresar a revisión un vehículo pendiente de autorización o disponible (no apartado ni vendido).");

            if (string.IsNullOrWhiteSpace(modelo.Motivo))
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Motivo), "Indica el motivo para regresarlo a revisión.");

            var estadoAnterior = vehiculo.Estado;
            var motivo = modelo.Motivo.Trim();

            var revision = ObtenerOCrear(vehiculo);
            revision.FechaFinalizacion = null;
            revision.FechaAutorizacion = null;
            revision.AutorizadoPor = null;
            revision.FechaActualizacion = DateTime.Now;
            revision.Notas.Add(NuevaNota(actor, $"Regresado a revisión por {actor.NombreCompleto}. Motivo: {motivo}", esDelSistema: true));

            vehiculo.Estado = EstadoVehiculo.EnRevision;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision,
                $"Regresó a revisión · {estadoAnterior.NombreVisible()} → En revisión/preparación · Motivo: {motivo}");
            return Guardar("El vehículo regresó a revisión / preparación.");
        }

        public async Task<ResultadoOperacion> SubirFotosAsync(SubirFotosViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeRevisar(actor.Rol.Tipo)) return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            var permitido = modelo.Tipo switch
            {
                TipoFoto.Revision => vehiculo.Estado == EstadoVehiculo.EnRevision,
                TipoFoto.Final => vehiculo.Estado is EstadoVehiculo.EnRevision or EstadoVehiculo.RevisionFinalizada or EstadoVehiculo.Disponible,
                _ => false
            };
            if (!permitido)
                return ResultadoOperacion.Error("En la etapa actual del vehículo no se pueden agregar este tipo de fotografías.");

            if (modelo.Archivos == null || modelo.Archivos.Count == 0)
                return ResultadoOperacion.Error("Selecciona al menos una fotografía.");

            var antes = vehiculo.Fotografias.Count;
            var resultado = await _fotos.GuardarAsync(vehiculo, modelo.Archivos, modelo.Tipo, actor);
            if (vehiculo.Fotografias.Count == antes) return resultado;      // no se agregó ninguna

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision,
                $"Agregó fotografías ({modelo.Tipo.NombreVisible().ToLower()})");

            var guardado = Guardar(resultado.Mensaje);
            return guardado.Exitoso ? resultado : guardado;
        }

        public List<NotaResumen> NotasRecientes(int usuarioId, int cantidad) =>
            _revisiones.NotasRecientes(usuarioId, cantidad)
                .Select(n => new NotaResumen
                {
                    VehiculoId = n.VehiculoId,
                    Vehiculo = n.Vehiculo,
                    Fecha = n.Fecha,
                    Texto = n.Texto
                })
                .ToList();

        /// <summary>La revisión del vehículo; si todavía no existe, se crea y se asocia al vehículo.</summary>
        private Revision ObtenerOCrear(Vehiculo vehiculo)
        {
            var revision = vehiculo.Revision ?? _revisiones.ObtenerPorVehiculo(vehiculo.Id);
            if (revision != null) return revision;

            revision = new Revision { Vehiculo = vehiculo, FechaActualizacion = DateTime.Now };
            _revisiones.Agregar(revision);
            return revision;
        }

        private static NotaRevision NuevaNota(Usuario actor, string texto, bool esDelSistema) => new()
        {
            UsuarioId = actor.Id,
            UsuarioNombre = actor.NombreCompleto,
            Fecha = DateTime.Now,
            Texto = texto.Length <= 500 ? texto : texto[..500],
            EsDelSistema = esDelSistema
        };
    }
}
