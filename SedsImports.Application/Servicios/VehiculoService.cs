using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Excepciones;
using SedsImports.Domain.Extensiones;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>
    /// Lógica de negocio de los vehículos: registro, edición, consultas, eliminación lógica y la
    /// autorización del dueño para pasar a "Disponible para venta" (CU-01, CU-03, CU-07).
    /// </summary>
    public class VehiculoService : ServicioBase, IVehiculoService
    {
        private readonly IVehiculoRepository _vehiculos;
        private readonly IApartadoRepository _apartados;
        private readonly IFotoService _fotos;
        private readonly IBitacoraService _bitacora;

        public VehiculoService(IUnidadDeTrabajo unidadDeTrabajo, IVehiculoRepository vehiculos,
            IApartadoRepository apartados, IFotoService fotos, IBitacoraService bitacora)
            : base(unidadDeTrabajo)
        {
            _vehiculos = vehiculos;
            _apartados = apartados;
            _fotos = fotos;
            _bitacora = bitacora;
        }

        // ───────────────────────── Consultas ─────────────────────────

        public List<VehiculoListadoDto> Listar(string? busqueda = null, EstadoVehiculo? estado = null, IEnumerable<EstadoVehiculo>? estadosVisibles = null) =>
            _vehiculos.Buscar(busqueda, estado, estadosVisibles).Select(ADto).ToList();

        public ConteoEstadosDto Conteos(IEnumerable<EstadoVehiculo>? estadosVisibles = null)
        {
            var porEstado = _vehiculos.ContarPorEstado(estadosVisibles);
            return new ConteoEstadosDto
            {
                Total = porEstado.Values.Sum(),
                PorEstado = porEstado.ToDictionary(p => p.Key, p => p.Value)
            };
        }

        public Vehiculo? Obtener(int id) => _vehiculos.ObtenerPorId(id);

        public Vehiculo? ObtenerParaHistorial(int id) => _vehiculos.ObtenerIncluyendoEliminados(id);

        public HashSet<int> IdsEliminados() => _vehiculos.IdsEliminados().ToHashSet();

        public int RegistradosDesde(DateTime desde) => _vehiculos.ContarRegistradosDesde(desde);

        /// <summary>Convierte el vehículo (con su documentación y revisión ya cargadas) en una fila de listado.</summary>
        public VehiculoListadoDto ADto(Vehiculo v)
        {
            var (accion, responsable) = FlujoVehiculo.SiguienteAccion(v.Estado);

            return new VehiculoListadoDto
            {
                Id = v.Id,
                Nombre = v.NombreCompleto,
                Vin = v.Vin,
                VinCorto = v.VinCorto,
                Color = v.Color,
                Estado = v.Estado,
                EstadoDocumentacion = v.Documentacion?.Estado ?? EstadoDocumentacion.Pendiente,
                ObservacionDocumentacion = v.Documentacion?.Observaciones,
                SiguienteAccion = accion,
                ResponsableSiguiente = responsable,
                FechaIngreso = v.FechaIngreso,
                CantidadNotas = v.Revision?.Notas.Count(n => !n.EsDelSistema) ?? 0,
                CantidadFotosFinales = v.Fotografias.Count(f => f.Tipo == TipoFoto.Final),
                IndiceColor = v.Id % 5
            };
        }

        // ───────────────────────── Operaciones ─────────────────────────

        /// <summary>
        /// Registra el vehículo con su expediente de documentos. Se guarda en dos pasos dentro de una
        /// transacción: primero el vehículo (para obtener su Id, que da nombre a la carpeta de fotos)
        /// y después las fotografías y la bitácora. Si algo falla, no queda nada a medias.
        /// </summary>
        public async Task<ResultadoOperacion<Vehiculo>> RegistrarAsync(VehiculoFormViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeRegistrarVehiculo(actor.Rol.Tipo))
                return ResultadoOperacion<Vehiculo>.Error(Permisos.SinPermiso);

            var validacion = ValidarDatos(modelo, null);
            if (!validacion.Exitoso) return ResultadoOperacion<Vehiculo>.Desde(validacion);

            var vehiculo = new Vehiculo
            {
                Marca = modelo.Marca.Trim(),
                Modelo = modelo.Modelo.Trim(),
                Anio = modelo.Anio!.Value,
                Color = modelo.Color.Trim(),
                Vin = modelo.Vin.Trim().ToUpperInvariant(),
                Detalles = string.IsNullOrWhiteSpace(modelo.Detalles) ? null : modelo.Detalles.Trim(),
                Estado = EstadoVehiculo.RecienLlegado,       // el estado inicial siempre lo asigna el sistema
                FechaIngreso = DateTime.Now,
                RegistradoPorId = actor.Id,
                RegistradoPorNombre = actor.NombreCompleto,
                // La documentación nace "Pendiente" para que la asistente la complete en paralelo.
                Documentacion = new Documentacion
                {
                    Estado = EstadoDocumentacion.Pendiente,
                    Observaciones = "Sin documentos registrados."
                }
            };

            try
            {
                using var transaccion = UnidadDeTrabajo.IniciarTransaccion();

                _vehiculos.Agregar(vehiculo);
                UnidadDeTrabajo.GuardarCambios();             // aquí el vehículo recibe su Id

                var fotos = await _fotos.GuardarAsync(vehiculo, modelo.Fotos, TipoFoto.Ingreso, actor);
                _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Registro, "Registró vehículo · Estado inicial: Recién llegado");
                UnidadDeTrabajo.GuardarCambios();

                transaccion.Confirmar();

                var mensaje = "Vehículo registrado correctamente.";
                if (!fotos.Exitoso) mensaje += " " + fotos.Mensaje;
                return ResultadoOperacion<Vehiculo>.Ok(vehiculo, mensaje);
            }
            catch (ErrorPersistenciaException ex)
            {
                return ResultadoOperacion<Vehiculo>.Error(ex.Message);
            }
        }

        public async Task<ResultadoOperacion> EditarAsync(VehiculoFormViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeEditarVehiculo(actor.Rol.Tipo))
                return ResultadoOperacion.Error(Permisos.SinPermiso);

            var vehiculo = modelo.Id.HasValue ? _vehiculos.ObtenerPorId(modelo.Id.Value) : null;
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            if (vehiculo.Estado == EstadoVehiculo.Vendido)
                return ResultadoOperacion.Error("Un vehículo vendido ya no se puede editar.");

            var validacion = ValidarDatos(modelo, vehiculo.Id);
            if (!validacion.Exitoso) return validacion;

            var cambios = new List<string>();
            void Cambiar(string campo, string? anterior, string? nuevo, Action aplicar)
            {
                if (!string.Equals(anterior ?? string.Empty, nuevo ?? string.Empty, StringComparison.Ordinal))
                {
                    cambios.Add(campo);
                    aplicar();
                }
            }

            var vinNuevo = modelo.Vin.Trim().ToUpperInvariant();
            var detallesNuevos = string.IsNullOrWhiteSpace(modelo.Detalles) ? null : modelo.Detalles.Trim();

            Cambiar("Marca", vehiculo.Marca, modelo.Marca.Trim(), () => vehiculo.Marca = modelo.Marca.Trim());
            Cambiar("Modelo", vehiculo.Modelo, modelo.Modelo.Trim(), () => vehiculo.Modelo = modelo.Modelo.Trim());
            Cambiar("Año", vehiculo.Anio.ToString(), modelo.Anio!.Value.ToString(), () => vehiculo.Anio = modelo.Anio!.Value);
            Cambiar("Color", vehiculo.Color, modelo.Color.Trim(), () => vehiculo.Color = modelo.Color.Trim());
            Cambiar("VIN", vehiculo.Vin, vinNuevo, () => vehiculo.Vin = vinNuevo);
            Cambiar("Detalles", vehiculo.Detalles, detallesNuevos, () => vehiculo.Detalles = detallesNuevos);

            var fotos = await _fotos.GuardarAsync(vehiculo, modelo.Fotos, TipoFoto.Ingreso, actor);
            if (fotos.Exitoso && fotos.Dato > 0) cambios.Add($"{fotos.Dato} fotografía(s) de ingreso");

            if (cambios.Count == 0)
                return ResultadoOperacion.Ok("No se detectaron cambios en el vehículo.");

            vehiculo.FechaUltimaEdicion = DateTime.Now;
            vehiculo.EditadoPorNombre = actor.NombreCompleto;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Edicion, "Editó datos del vehículo: " + string.Join(", ", cambios));

            var mensaje = "Datos del vehículo actualizados.";
            if (!fotos.Exitoso) mensaje += " " + fotos.Mensaje;
            return Guardar(mensaje);
        }

        public ResultadoOperacion AutorizarDisponibilidad(int vehiculoId, Usuario actor)
        {
            if (!Permisos.PuedeAutorizar(actor.Rol.Tipo))
                return ResultadoOperacion.Error("Solo el dueño puede autorizar que un vehículo quede disponible para la venta.");

            var vehiculo = _vehiculos.ObtenerPorId(vehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            // CU-03, flujo alterno: si la revisión no está finalizada, no se permite el cambio.
            if (vehiculo.Estado != EstadoVehiculo.RevisionFinalizada)
                return ResultadoOperacion.Error("Solo se puede autorizar un vehículo cuya revisión ya fue finalizada por el mecánico.");

            if (vehiculo.Revision != null)
            {
                vehiculo.Revision.FechaAutorizacion = DateTime.Now;
                vehiculo.Revision.AutorizadoPor = actor.NombreCompleto;
                vehiculo.Revision.FechaActualizacion = DateTime.Now;
            }

            vehiculo.Estado = EstadoVehiculo.Disponible;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Autorizacion,
                "Autorizó disponibilidad · Pendiente de autorización → Disponible para venta");

            return Guardar($"{vehiculo.NombreCompleto} quedó disponible para la venta.");
        }

        /// <summary>
        /// Eliminación lógica (solo el dueño). El vehículo desaparece de listados, tableros y búsquedas,
        /// pero sus datos y su historial se conservan. Un vehículo vendido no se elimina porque
        /// forma parte del historial de ventas; si estaba apartado, el apartado se cancela.
        /// </summary>
        public ResultadoOperacion Eliminar(int vehiculoId, Usuario actor)
        {
            if (!Permisos.PuedeEliminarVehiculo(actor.Rol.Tipo))
                return ResultadoOperacion.Error("Solo el dueño puede eliminar vehículos.");

            var vehiculo = _vehiculos.ObtenerPorId(vehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe o ya fue eliminado.");

            if (vehiculo.Estado == EstadoVehiculo.Vendido)
                return ResultadoOperacion.Error("Un vehículo vendido forma parte del historial de ventas y no se puede eliminar.");

            var estadoAnterior = vehiculo.Estado;

            var apartado = _apartados.ObtenerActivoPorVehiculo(vehiculoId);
            if (apartado != null)
            {
                apartado.Estado = EstadoApartado.Cancelado;
                apartado.FechaCierre = DateTime.Now;
                apartado.MotivoCancelacion = "El vehículo fue eliminado del sistema.";
                _apartados.Actualizar(apartado);
            }

            vehiculo.FechaEliminacion = DateTime.Now;
            vehiculo.EliminadoPorNombre = actor.NombreCompleto;
            _vehiculos.Eliminar(vehiculo);                   // eliminación lógica

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Eliminacion,
                $"Eliminó el vehículo · VIN {vehiculo.Vin} · Estado al eliminarlo: {estadoAnterior.NombreVisible()}"
                + (apartado != null ? " · Se canceló su apartado" : ""));

            return Guardar($"{vehiculo.NombreCompleto} se eliminó del sistema.");
        }

        /// <summary>Reglas comunes de registro y edición: año válido y VIN único (CU-01, flujo alterno).</summary>
        private ResultadoOperacion ValidarDatos(VehiculoFormViewModel modelo, int? idActual)
        {
            var anioMaximo = DateTime.Today.Year + 1;
            if (modelo.Anio is null || modelo.Anio < 1950 || modelo.Anio > anioMaximo)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Anio), $"El año debe estar entre 1950 y {anioMaximo}.");

            var existente = _vehiculos.ObtenerPorVin(modelo.Vin);
            if (existente != null && existente.Id != idActual)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Vin),
                    $"Ya existe un vehículo con este VIN ({existente.NombreCompleto}). Verifica los datos.");

            return ResultadoOperacion.Ok(string.Empty);
        }
    }
}
