using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>
    /// Vehículos: listado, ficha, registro, edición y autorización de disponibilidad.
    /// Usa rutas por atributo: /vehiculos, /vehiculos/registrar, /vehiculos/5, /vehiculos/5/editar...
    /// </summary>
    [Route("vehiculos")]
    [PerfilRequerido]
    public class VehiculosController : BaseController
    {
        private readonly IVehiculoService _vehiculos;
        private readonly IRevisionService _revisiones;
        private readonly IDocumentacionService _documentacion;
        private readonly IApartadoService _apartados;
        private readonly IVentaService _ventas;
        private readonly IClienteService _clientes;
        private readonly IUsuarioService _usuarios;

        public VehiculosController(IVehiculoService vehiculos, IRevisionService revisiones,
            IDocumentacionService documentacion, IApartadoService apartados, IVentaService ventas,
            IClienteService clientes, IUsuarioService usuarios)
        {
            _vehiculos = vehiculos;
            _revisiones = revisiones;
            _documentacion = documentacion;
            _apartados = apartados;
            _ventas = ventas;
            _clientes = clientes;
            _usuarios = usuarios;
        }

        // GET /vehiculos?busqueda=kia&estado=RecienLlegado
        [HttpGet("")]
        public IActionResult Index(string? busqueda, EstadoVehiculo? estado)
        {
            var rol = RolActual;
            var visibles = Permisos.EstadosVisibles(rol);

            var modelo = new VehiculoListadoViewModel
            {
                Busqueda = busqueda,
                EstadoFiltro = estado,
                EstadosVisibles = visibles.ToList(),
                Vehiculos = _vehiculos.Listar(busqueda, estado, visibles),
                Conteos = _vehiculos.Conteos(visibles),
                MostrarDocumentacion = rol is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor,
                PuedeRegistrar = Permisos.PuedeRegistrarVehiculo(rol),
                Subtitulo = rol switch
                {
                    TipoRol.Mecanico => "Consulta los vehículos por trabajar, en trabajo y ya preparados.",
                    TipoRol.DuenoVendedor => "Consulta y administra todos los vehículos.",
                    _ => "Consulta el estado de cada vehículo y ábrelo para ver su ficha."
                }
            };

            return View(modelo);
        }

        // GET /vehiculos/5
        [HttpGet("{id:int}")]
        public IActionResult Detalle(int id, string? origen = null)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NotFound();

            var rol = RolActual;
            if (!Permisos.EstadosVisibles(rol).Contains(vehiculo.Estado))
            {
                MostrarError("Tu perfil no tiene acceso a la ficha de este vehículo.");
                return RedirectToAction(nameof(Index));
            }

            return View(ArmarDetalle(vehiculo, rol, origen));
        }

        // GET /vehiculos/registrar
        [HttpGet("registrar")]
        [PerfilRequerido(TipoRol.EncargadoPatio, TipoRol.DuenoVendedor)]
        public IActionResult Create() => View(new VehiculoFormViewModel());

        // POST /vehiculos/registrar
        [HttpPost("registrar")]
        [PerfilRequerido(TipoRol.EncargadoPatio, TipoRol.DuenoVendedor)]
        public async Task<IActionResult> Create(VehiculoFormViewModel formulario)
        {
            if (!ModelState.IsValid) return View(formulario);

            var resultado = await _vehiculos.RegistrarAsync(formulario, UsuarioActual);
            if (!resultado.Exitoso || resultado.Dato == null)
            {
                AgregarErrores(resultado);
                return View(formulario);
            }

            if (resultado.Mensaje.Contains("omitieron")) MostrarError(resultado.Mensaje);
            return RedirectToAction(nameof(Registrado), new { id = resultado.Dato.Id });
        }

        // GET /vehiculos/5/registrado
        [HttpGet("{id:int}/registrado")]
        [PerfilRequerido(TipoRol.EncargadoPatio, TipoRol.DuenoVendedor)]
        public IActionResult Registrado(int id)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NotFound();

            return View(new VehiculoRegistradoViewModel
            {
                Id = vehiculo.Id,
                Nombre = vehiculo.NombreCompleto,
                Vin = vehiculo.Vin,
                Estado = vehiculo.Estado
            });
        }

        // GET /vehiculos/5/editar
        [HttpGet("{id:int}/editar")]
        [PerfilRequerido(TipoRol.EncargadoPatio, TipoRol.DuenoVendedor)]
        public IActionResult Editar(int id)
        {
            var vehiculo = _vehiculos.Obtener(id);
            if (vehiculo == null) return NotFound();

            if (vehiculo.Estado == EstadoVehiculo.Vendido)
            {
                MostrarError("Un vehículo vendido ya no se puede editar.");
                return RedirectToAction(nameof(Detalle), new { id });
            }

            return View(new VehiculoFormViewModel
            {
                Id = vehiculo.Id,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Anio = vehiculo.Anio,
                Color = vehiculo.Color,
                Vin = vehiculo.Vin,
                Detalles = vehiculo.Detalles,
                EstadoActual = vehiculo.Estado
            });
        }

        // POST /vehiculos/5/editar
        [HttpPost("{id:int}/editar")]
        [PerfilRequerido(TipoRol.EncargadoPatio, TipoRol.DuenoVendedor)]
        public async Task<IActionResult> Editar(int id, VehiculoFormViewModel formulario)
        {
            formulario.Id = id;
            formulario.EstadoActual = _vehiculos.Obtener(id)?.Estado;
            if (!ModelState.IsValid) return View(formulario);

            var resultado = await _vehiculos.EditarAsync(formulario, UsuarioActual);
            if (!resultado.Exitoso)
            {
                AgregarErrores(resultado);
                return View(formulario);
            }

            MostrarExito(resultado.Mensaje);
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // POST /vehiculos/5/autorizar  — el dueño autoriza que quede disponible (CU-03)
        [HttpPost("{id:int}/autorizar")]
        [PerfilRequerido(TipoRol.DuenoVendedor)]
        public IActionResult Autorizar(int id)
        {
            MostrarResultado(_vehiculos.AutorizarDisponibilidad(id, UsuarioActual));
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // ───────────────────────── Armado de la ficha ─────────────────────────

        private VehiculoDetalleViewModel ArmarDetalle(Vehiculo vehiculo, TipoRol rol, string? origen)
        {
            var revision = _revisiones.ObtenerPorVehiculo(vehiculo.Id);
            var documentacion = _documentacion.ObtenerPorVehiculo(vehiculo.Id);
            var apartado = _apartados.ObtenerActivo(vehiculo.Id);
            var venta = _ventas.ObtenerPorVehiculo(vehiculo.Id);
            var clienteApartado = apartado != null ? _clientes.Obtener(apartado.ClienteId) : null;
            var clienteVenta = venta != null ? _clientes.Obtener(venta.ClienteId) : null;

            var (accion, responsable) = VehiculoService.SiguienteAccion(vehiculo.Estado);

            var modelo = new VehiculoDetalleViewModel
            {
                Vehiculo = vehiculo,
                Revision = revision,
                Documentacion = documentacion,
                Apartado = apartado,
                ClienteApartado = clienteApartado,
                Venta = venta,
                ClienteVenta = clienteVenta,
                Rol = rol,
                NombreUsuario = UsuarioActual.NombreCompleto,
                Origen = origen,
                SiguienteAccion = accion,
                ResponsableSiguiente = responsable,
                SiguienteMeCorresponde = (responsable == "Mecánico" && rol == TipoRol.Mecanico)
                                         || (responsable == "Dueño" && rol == TipoRol.DuenoVendedor),
                ResponsableActual = ResponsableActual(vehiculo, revision, rol),
                PasosProceso = PasosProceso(vehiculo.Estado, rol),
                PasosDocumentacion = new List<PasoProceso>
                {
                    new() { Numero = 1, Titulo = "Pendiente", Completado = documentacion.Estado == EstadoDocumentacion.Completa, Actual = documentacion.Estado == EstadoDocumentacion.Pendiente },
                    new() { Numero = 2, Titulo = "Completa", Completado = documentacion.Estado == EstadoDocumentacion.Completa }
                },
                NuevaNota = new NotaRevisionViewModel { VehiculoId = vehiculo.Id },
                NuevoApartado = new ApartadoViewModel { VehiculoId = vehiculo.Id, Vehiculo = vehiculo.NombreCompleto, Origen = "ficha" },
                NuevaVenta = new VentaViewModel
                {
                    VehiculoId = vehiculo.Id,
                    Vehiculo = vehiculo.NombreCompleto,
                    VieneDeApartado = apartado != null,
                    AnticipoPrevio = apartado?.Anticipo,
                    NombreCliente = clienteApartado?.Nombre ?? string.Empty,
                    Telefono = clienteApartado?.Telefono,
                    FechaVenta = DateTime.Today,
                    Origen = "ficha"
                },
                FormDocumentacion = _documentacion.ArmarFormulario(vehiculo.Id)
            };
            modelo.FormDocumentacion.Origen = "ficha";
            return modelo;
        }

        private string ResponsableActual(Vehiculo vehiculo, Revision? revision, TipoRol rol)
        {
            string NombreDe(TipoRol tipo) =>
                _usuarios.Listar().FirstOrDefault(u => u.Rol.Tipo == tipo && u.Activo)?.NombreCompleto ?? "por asignar";

            return vehiculo.Estado switch
            {
                EstadoVehiculo.RecienLlegado when rol == TipoRol.EncargadoPatio => $"Patio · {vehiculo.RegistradoPorNombre}",
                EstadoVehiculo.RecienLlegado => $"Mecánico · {NombreDe(TipoRol.Mecanico)}",
                EstadoVehiculo.EnRevision => $"Mecánico · {revision?.MecanicoNombre ?? NombreDe(TipoRol.Mecanico)}",
                EstadoVehiculo.RevisionFinalizada => $"Dueño / vendedor · {NombreDe(TipoRol.DuenoVendedor)} (autorizar disponibilidad)",
                EstadoVehiculo.Disponible or EstadoVehiculo.Apartado => $"Dueño / vendedor · {NombreDe(TipoRol.DuenoVendedor)}",
                _ => "Proceso cerrado"
            };
        }

        /// <summary>
        /// Patio y mecánico ven el flujo técnico (4 pasos); asistente y dueño ven el proceso completo (6 pasos).
        /// </summary>
        private static List<PasoProceso> PasosProceso(EstadoVehiculo estado, TipoRol rol)
        {
            var titulos = new List<(EstadoVehiculo Estado, string Titulo)>
            {
                (EstadoVehiculo.RecienLlegado, "Recién llegado"),
                (EstadoVehiculo.EnRevision, "En revisión / preparación"),
                (EstadoVehiculo.RevisionFinalizada, "Revisión finalizada · por autorizar"),
                (EstadoVehiculo.Disponible, "Disponible para venta")
            };

            if (rol is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor)
            {
                titulos.Add((EstadoVehiculo.Apartado, "Apartado"));
                titulos.Add((EstadoVehiculo.Vendido, "Vendido"));
            }

            // Para el flujo técnico, apartado y vendido cuentan como "después de disponible".
            var estadoEfectivo = titulos.Any(t => t.Estado == estado) ? estado : EstadoVehiculo.Disponible;
            var pasoFinalTecnico = !titulos.Any(t => t.Estado == estado);

            return titulos.Select((t, i) => new PasoProceso
            {
                Numero = i + 1,
                Titulo = t.Titulo,
                Completado = t.Estado < estadoEfectivo
                             || (pasoFinalTecnico && t.Estado == estadoEfectivo)
                             || (estado == EstadoVehiculo.Vendido && t.Estado == EstadoVehiculo.Vendido),
                Actual = t.Estado == estadoEfectivo && !pasoFinalTecnico
            }).ToList();
        }
    }
}
