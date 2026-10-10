using SedsImports.Web.DTOs;
using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>
    /// Lógica de negocio de los vehículos: registro, edición, consultas y la
    /// autorización del dueño para pasar a "Disponible para venta" (CU-01, CU-03, CU-07).
    /// </summary>
    public class VehiculoService : IVehiculoService
    {
        private readonly IVehiculoRepository _vehiculos;
        private readonly IDocumentacionRepository _documentos;
        private readonly IRevisionRepository _revisiones;
        private readonly IFotoService _fotos;
        private readonly IBitacoraService _bitacora;

        public VehiculoService(IVehiculoRepository vehiculos, IDocumentacionRepository documentos,
            IRevisionRepository revisiones, IFotoService fotos, IBitacoraService bitacora)
        {
            _vehiculos = vehiculos;
            _documentos = documentos;
            _revisiones = revisiones;
            _fotos = fotos;
            _bitacora = bitacora;
        }

        public List<VehiculoListadoDto> Listar(string? busqueda = null, EstadoVehiculo? estado = null, IEnumerable<EstadoVehiculo>? estadosVisibles = null)
        {
            IEnumerable<Vehiculo> consulta = _vehiculos.ObtenerTodos();

            if (estadosVisibles != null)
            {
                var visibles = estadosVisibles.ToHashSet();
                consulta = consulta.Where(v => visibles.Contains(v.Estado));
            }

            if (estado.HasValue)
                consulta = consulta.Where(v => v.Estado == estado.Value);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(v =>
                    v.Marca.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    v.Modelo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    v.Vin.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    v.Anio.ToString() == texto ||
                    v.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            return consulta
                .OrderByDescending(v => v.FechaIngreso)
                .Select(ADto)
                .ToList();
        }

        public ConteoEstadosDto Conteos(IEnumerable<EstadoVehiculo>? estadosVisibles = null)
        {
            var todos = _vehiculos.ObtenerTodos().AsEnumerable();
            if (estadosVisibles != null)
            {
                var visibles = estadosVisibles.ToHashSet();
                todos = todos.Where(v => visibles.Contains(v.Estado));
            }

            var lista = todos.ToList();
            return new ConteoEstadosDto
            {
                Total = lista.Count,
                PorEstado = lista.GroupBy(v => v.Estado).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public Vehiculo? Obtener(int id) => _vehiculos.ObtenerPorId(id);

        public int RegistradosDesde(DateTime desde) => _vehiculos.ObtenerTodos().Count(v => v.FechaIngreso >= desde);

        public VehiculoListadoDto ADto(Vehiculo v)
        {
            var doc = _documentos.ObtenerPorVehiculo(v.Id);
            var revision = _revisiones.ObtenerPorVehiculo(v.Id);
            var (accion, responsable) = SiguienteAccion(v.Estado);

            return new VehiculoListadoDto
            {
                Id = v.Id,
                Nombre = v.NombreCompleto,
                Vin = v.Vin,
                VinCorto = v.VinCorto,
                Color = v.Color,
                Estado = v.Estado,
                EstadoDocumentacion = doc?.Estado ?? EstadoDocumentacion.Pendiente,
                ObservacionDocumentacion = doc?.Observaciones,
                SiguienteAccion = accion,
                ResponsableSiguiente = responsable,
                FechaIngreso = v.FechaIngreso,
                CantidadNotas = revision?.Notas.Count(n => !n.EsDelSistema) ?? 0,
                CantidadFotosFinales = v.Fotografias.Count(f => f.Tipo == TipoFoto.Final),
                IndiceColor = v.Id % 5
            };
        }

        /// <summary>Qué sigue en el proceso y quién debe hacerlo.</summary>
        public static (string? Accion, string? Responsable) SiguienteAccion(EstadoVehiculo estado) => estado switch
        {
            EstadoVehiculo.RecienLlegado => ("Iniciar revisión", "Mecánico"),
            EstadoVehiculo.EnRevision => ("Finalizar preparación", "Mecánico"),
            EstadoVehiculo.RevisionFinalizada => ("Autorizar disponibilidad", "Dueño"),
            EstadoVehiculo.Apartado => ("Registrar venta", "Dueño"),
            _ => (null, null)
        };

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
                RegistradoPorNombre = actor.NombreCompleto
            };

            _vehiculos.Agregar(vehiculo);

            // La documentación nace "Pendiente" para que la asistente la complete en paralelo.
            _documentos.Agregar(new Documentacion
            {
                VehiculoId = vehiculo.Id,
                Estado = EstadoDocumentacion.Pendiente,
                Observaciones = "Sin documentos registrados."
            });

            var fotos = await _fotos.GuardarAsync(vehiculo, modelo.Fotos, TipoFoto.Ingreso, actor);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Registro, "Registró vehículo · Estado inicial: Recién llegado");

            var mensaje = "Vehículo registrado correctamente.";
            if (!fotos.Exitoso) mensaje += " " + fotos.Mensaje;
            return ResultadoOperacion<Vehiculo>.Ok(vehiculo, mensaje);
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
            return ResultadoOperacion.Ok(mensaje);
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

            var revision = _revisiones.ObtenerPorVehiculo(vehiculoId);
            if (revision != null)
            {
                revision.FechaAutorizacion = DateTime.Now;
                revision.AutorizadoPor = actor.NombreCompleto;
                revision.FechaActualizacion = DateTime.Now;
                _revisiones.Actualizar(revision);
            }

            vehiculo.Estado = EstadoVehiculo.Disponible;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Autorizacion,
                "Autorizó disponibilidad · Pendiente de autorización → Disponible para venta");

            return ResultadoOperacion.Ok($"{vehiculo.NombreCompleto} quedó disponible para la venta.");
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

    /// <summary>
    /// Guarda las fotografías en wwwroot/uploads/vehiculos/{id}. En la Fase 3 se puede
    /// cambiar por un almacenamiento en la nube sin tocar los controladores.
    /// </summary>
    public class FotoService : IFotoService
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximo = 5 * 1024 * 1024;
        private const int MaximoPorCarga = 10;

        private readonly IWebHostEnvironment _entorno;
        private readonly IVehiculoRepository _vehiculos;

        public FotoService(IWebHostEnvironment entorno, IVehiculoRepository vehiculos)
        {
            _entorno = entorno;
            _vehiculos = vehiculos;
        }

        public async Task<ResultadoOperacion<int>> GuardarAsync(Vehiculo vehiculo, IEnumerable<IFormFile>? archivos, TipoFoto tipo, Usuario actor)
        {
            var lista = archivos?.Where(a => a is { Length: > 0 }).ToList() ?? new List<IFormFile>();
            if (lista.Count == 0) return ResultadoOperacion<int>.Ok(0, string.Empty);

            if (lista.Count > MaximoPorCarga)
                return ResultadoOperacion<int>.Error($"Puedes subir como máximo {MaximoPorCarga} fotografías a la vez.");

            var carpetaRelativa = Path.Combine("uploads", "vehiculos", vehiculo.Id.ToString());
            var carpeta = Path.Combine(_entorno.WebRootPath, carpetaRelativa);
            Directory.CreateDirectory(carpeta);

            var guardadas = 0;
            var omitidas = 0;
            foreach (var archivo in lista)
            {
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension) || archivo.Length > TamanoMaximo)
                {
                    omitidas++;
                    continue;
                }

                var nombre = $"{tipo.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}{extension}";
                await using (var destino = File.Create(Path.Combine(carpeta, nombre)))
                {
                    await archivo.CopyToAsync(destino);
                }

                vehiculo.Fotografias.Add(new Fotografia
                {
                    Id = _vehiculos.SiguienteIdFoto(),
                    Tipo = tipo,
                    Ruta = "/" + carpetaRelativa.Replace('\\', '/') + "/" + nombre,
                    Descripcion = Path.GetFileNameWithoutExtension(archivo.FileName),
                    Fecha = DateTime.Now,
                    SubidaPor = actor.NombreCompleto
                });
                guardadas++;
            }

            _vehiculos.Actualizar(vehiculo);

            return omitidas == 0
                ? ResultadoOperacion<int>.Ok(guardadas, $"{guardadas} fotografía(s) agregada(s).")
                : ResultadoOperacion<int>.Error($"Se omitieron {omitidas} archivo(s): solo se aceptan JPG, PNG o WEBP de hasta 5 MB.");
        }
    }
}
