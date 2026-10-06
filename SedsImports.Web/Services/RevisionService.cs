using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>
    /// Flujo técnico del mecánico (RF-05, CU-02):
    /// Recién llegado → En revisión/preparación → Pendiente de autorización.
    /// Finalizar la revisión NO pone el vehículo disponible: eso lo decide el dueño.
    /// </summary>
    public class RevisionService : IRevisionService
    {
        private readonly IRevisionRepository _revisiones;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IFotoService _fotos;
        private readonly IBitacoraService _bitacora;

        public RevisionService(IRevisionRepository revisiones, IVehiculoRepository vehiculos,
            IFotoService fotos, IBitacoraService bitacora)
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

            var revision = ObtenerOCrear(vehiculoId);
            revision.MecanicoId = actor.Id;
            revision.MecanicoNombre = actor.NombreCompleto;
            revision.FechaInicio = DateTime.Now;
            revision.FechaActualizacion = DateTime.Now;
            _revisiones.Actualizar(revision);

            vehiculo.Estado = EstadoVehiculo.EnRevision;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, "Inició revisión · Recién llegado → En revisión/preparación");
            return ResultadoOperacion.Ok("Revisión iniciada. Registra cada trabajo en las notas.");
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

            var revision = ObtenerOCrear(vehiculo.Id);
            var texto = modelo.Texto.Trim();
            revision.Notas.Add(new NotaRevision
            {
                Id = _revisiones.SiguienteIdNota(),
                UsuarioId = actor.Id,
                UsuarioNombre = actor.NombreCompleto,
                Fecha = DateTime.Now,
                Texto = texto
            });
            revision.FechaActualizacion = DateTime.Now;
            _revisiones.Actualizar(revision);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, $"Agregó anotación: {texto}");
            return ResultadoOperacion.Ok("Anotación agregada.");
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

            var revision = ObtenerOCrear(vehiculo.Id);
            var pendientes = string.IsNullOrWhiteSpace(modelo.Pendientes) ? "Ninguno." : modelo.Pendientes.Trim();

            revision.ResumenFinal = modelo.ResumenFinal.Trim();
            revision.Pendientes = pendientes;
            revision.FechaFinalizacion = DateTime.Now;
            revision.FechaAutorizacion = null;
            revision.AutorizadoPor = null;
            revision.FechaActualizacion = DateTime.Now;
            revision.Notas.Add(new NotaRevision
            {
                Id = _revisiones.SiguienteIdNota(),
                UsuarioId = actor.Id,
                UsuarioNombre = actor.NombreCompleto,
                Fecha = DateTime.Now,
                Texto = $"Revisión finalizada. Trabajos: {revision.ResumenFinal} Pendientes: {pendientes}",
                EsDelSistema = true
            });
            _revisiones.Actualizar(revision);

            vehiculo.Estado = EstadoVehiculo.RevisionFinalizada;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision, "Finalizó revisión · En revisión → Pendiente de autorización");
            return ResultadoOperacion.Ok("Revisión finalizada. El vehículo queda pendiente de que el dueño autorice su disponibilidad para la venta.");
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

            var revision = ObtenerOCrear(vehiculo.Id);
            revision.FechaFinalizacion = null;
            revision.FechaAutorizacion = null;
            revision.AutorizadoPor = null;
            revision.FechaActualizacion = DateTime.Now;
            revision.Notas.Add(new NotaRevision
            {
                Id = _revisiones.SiguienteIdNota(),
                UsuarioId = actor.Id,
                UsuarioNombre = actor.NombreCompleto,
                Fecha = DateTime.Now,
                Texto = $"Regresado a revisión por {actor.NombreCompleto}. Motivo: {motivo}",
                EsDelSistema = true
            });
            _revisiones.Actualizar(revision);

            vehiculo.Estado = EstadoVehiculo.EnRevision;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision,
                $"Regresó a revisión · {estadoAnterior.NombreVisible()} → En revisión/preparación · Motivo: {motivo}");
            return ResultadoOperacion.Ok("El vehículo regresó a revisión / preparación.");
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

            var resultado = await _fotos.GuardarAsync(vehiculo, modelo.Archivos, modelo.Tipo, actor);
            if (resultado.Dato > 0 || resultado.Exitoso)
            {
                _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Revision,
                    $"Agregó fotografías ({modelo.Tipo.NombreVisible().ToLower()})");
            }
            return resultado;
        }

        public List<NotaResumen> NotasRecientes(int usuarioId, int cantidad)
        {
            var vehiculos = _vehiculos.ObtenerTodos().ToDictionary(v => v.Id);
            return _revisiones.ObtenerTodos()
                .SelectMany(r => r.Notas.Where(n => n.UsuarioId == usuarioId && !n.EsDelSistema)
                    .Select(n => new NotaResumen
                    {
                        VehiculoId = r.VehiculoId,
                        Vehiculo = vehiculos.TryGetValue(r.VehiculoId, out var v) ? v.NombreCompleto : "—",
                        Fecha = n.Fecha,
                        Texto = n.Texto
                    }))
                .OrderByDescending(n => n.Fecha)
                .Take(cantidad)
                .ToList();
        }

        private Revision ObtenerOCrear(int vehiculoId) =>
            _revisiones.ObtenerPorVehiculo(vehiculoId) ?? _revisiones.Agregar(new Revision { VehiculoId = vehiculoId });
    }
}
