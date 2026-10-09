using System.ComponentModel.DataAnnotations;
using SedsImports.Web.DTOs;
using SedsImports.Web.Models;

namespace SedsImports.Web.ViewModels
{
    /// <summary>
    /// Formulario para registrar o editar un vehículo. Se usa tanto en Create como en Editar
    /// (Model Binding: el POST llena este objeto automáticamente con los campos del formulario).
    /// </summary>
    public class VehiculoFormViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Marca")]
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(40, ErrorMessage = "Máximo {1} caracteres.")]
        public string Marca { get; set; } = string.Empty;

        [Display(Name = "Modelo")]
        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(40, ErrorMessage = "Máximo {1} caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "Año")]
        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(1950, 2100, ErrorMessage = "Ingresa un año entre {1} y {2}.")]
        public int? Anio { get; set; }

        [Display(Name = "Color")]
        [Required(ErrorMessage = "El color es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo {1} caracteres.")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "VIN")]
        [Required(ErrorMessage = "El VIN es obligatorio.")]
        [StringLength(17, MinimumLength = 11, ErrorMessage = "El VIN debe tener entre {2} y {1} caracteres.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "El VIN solo admite letras y números, sin espacios.")]
        public string Vin { get; set; } = string.Empty;

        [Display(Name = "Detalles / observaciones generales")]
        [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
        public string? Detalles { get; set; }

        [Display(Name = "Fotografías del vehículo al momento del ingreso (opcional)")]
        public List<IFormFile>? Fotos { get; set; }

        public bool EsEdicion => Id.HasValue;

        /// <summary>Solo para edición: muestra el estado actual sin permitir cambiarlo desde aquí.</summary>
        public EstadoVehiculo? EstadoActual { get; set; }
    }

    /// <summary>Datos de la pantalla Vehículos (listado con búsqueda y chips de estado).</summary>
    public class VehiculoListadoViewModel
    {
        public string? Busqueda { get; set; }
        public EstadoVehiculo? EstadoFiltro { get; set; }
        public List<VehiculoListadoDto> Vehiculos { get; set; } = new();
        public ConteoEstadosDto Conteos { get; set; } = new();
        public List<EstadoVehiculo> EstadosVisibles { get; set; } = new();
        public bool MostrarDocumentacion { get; set; }
        public bool PuedeRegistrar { get; set; }
        public string Subtitulo { get; set; } = string.Empty;
    }

    /// <summary>Pantalla de confirmación después de registrar un vehículo.</summary>
    public class VehiculoRegistradoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public EstadoVehiculo Estado { get; set; }
    }

    /// <summary>Paso del indicador de progreso (stepper) que se muestra en la ficha.</summary>
    public class PasoProceso
    {
        public int Numero { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public bool Completado { get; set; }
        public bool Actual { get; set; }
    }

    /// <summary>
    /// Ficha del vehículo. Un mismo ViewModel sirve a los cuatro perfiles:
    /// la vista decide qué secciones y botones mostrar según el rol.
    /// </summary>
    public class VehiculoDetalleViewModel
    {
        public Vehiculo Vehiculo { get; set; } = null!;
        public Revision? Revision { get; set; }
        public Documentacion Documentacion { get; set; } = null!;
        public Apartado? Apartado { get; set; }
        public Cliente? ClienteApartado { get; set; }
        public Venta? Venta { get; set; }
        public Cliente? ClienteVenta { get; set; }

        public TipoRol Rol { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        public List<PasoProceso> PasosProceso { get; set; } = new();
        public List<PasoProceso> PasosDocumentacion { get; set; } = new();

        public string ResponsableActual { get; set; } = string.Empty;
        public string? SiguienteAccion { get; set; }
        public string? ResponsableSiguiente { get; set; }
        public bool SiguienteMeCorresponde { get; set; }

        public string? Origen { get; set; }

        // Formularios que se muestran en la ficha (se llenan con valores iniciales).
        public NotaRevisionViewModel NuevaNota { get; set; } = new();
        public ApartadoViewModel NuevoApartado { get; set; } = new();
        public VentaViewModel NuevaVenta { get; set; } = new();
        public DocumentacionViewModel FormDocumentacion { get; set; } = new();

        public bool EsPatio => Rol == TipoRol.EncargadoPatio;
        public bool EsMecanico => Rol == TipoRol.Mecanico;
        public bool EsAsistente => Rol == TipoRol.AsistenteAdministrativa;
        public bool EsDueno => Rol == TipoRol.DuenoVendedor;

        public bool PuedeEditar => (EsPatio || EsDueno) && Vehiculo.Estado != EstadoVehiculo.Vendido;

        public IEnumerable<Fotografia> Fotos(TipoFoto tipo) => Vehiculo.Fotografias.Where(f => f.Tipo == tipo);
    }

    /// <summary>Panel de inicio. Cada perfil usa solo las propiedades que le corresponden.</summary>
    public class InicioViewModel
    {
        public Usuario Usuario { get; set; } = null!;
        public TipoRol Rol => Usuario.Rol.Tipo;
        public DateTime Hoy { get; set; } = DateTime.Today;

        public ConteoEstadosDto Conteos { get; set; } = new();

        // Encargado de patio
        public int RegistradosHoy { get; set; }
        public int RegistradosSemana { get; set; }
        public List<VehiculoListadoDto> UltimosIngresos { get; set; } = new();

        // Listas por estado (patio, mecánico, asistente)
        public List<VehiculoListadoDto> RecienLlegados { get; set; } = new();
        public List<VehiculoListadoDto> EnRevision { get; set; } = new();
        public List<VehiculoListadoDto> PreparacionTerminada { get; set; } = new();
        public List<VehiculoListadoDto> Disponibles { get; set; } = new();
        public List<VehiculoListadoDto> PorAutorizar { get; set; } = new();
        public List<NotaResumen> MisNotas { get; set; } = new();

        // Asistente
        public int DocumentacionPendiente { get; set; }
        public int DocumentacionCompleta { get; set; }
        public List<VehiculoListadoDto> ConDocumentacionPendiente { get; set; } = new();

        // Dueño
        public int VendidosMes { get; set; }
        public decimal VentasMes { get; set; }
        public List<MovimientoBitacora> ActividadReciente { get; set; } = new();
    }

    /// <summary>Anotación resumida para "Mis últimas anotaciones" del mecánico.</summary>
    public class NotaResumen
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Texto { get; set; } = string.Empty;
    }
}
