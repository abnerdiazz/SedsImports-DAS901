using System.ComponentModel.DataAnnotations;
using SedsImports.Web.DTOs;
using SedsImports.Web.Models;

namespace SedsImports.Web.ViewModels
{
    /// <summary>Inicio de sesión con usuario (correo) y contraseña.</summary>
    public class LoginViewModel
    {
        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "Ingresa tu usuario.")]
        [EmailAddress(ErrorMessage = "El usuario es tu correo de SED's Imports.")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Ingresa tu contraseña.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    // ───────────────────────── Revisión (mecánico) ─────────────────────────

    /// <summary>Nueva anotación de revisión (CU-02: si no hay observaciones, se pide completarlas).</summary>
    public class NotaRevisionViewModel
    {
        public int VehiculoId { get; set; }

        [Display(Name = "Nueva anotación")]
        [Required(ErrorMessage = "Escribe la anotación antes de guardarla.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "La anotación debe tener entre {2} y {1} caracteres.")]
        public string Texto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cierre de la revisión. El mecánico deja su resumen y los últimos apuntes de lo que le
    /// falta al vehículo. El vehículo queda "Pendiente de autorización" hasta que el dueño decida.
    /// </summary>
    public class FinalizarRevisionViewModel : IValidatableObject
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public List<NotaRevision> NotasPrevias { get; set; } = new();
        public int FotosFinales { get; set; }

        [Display(Name = "Resumen de trabajos realizados")]
        [Required(ErrorMessage = "Describe brevemente los trabajos realizados.")]
        [StringLength(800, MinimumLength = 5, ErrorMessage = "El resumen debe tener entre {2} y {1} caracteres.")]
        public string ResumenFinal { get; set; } = string.Empty;

        [Display(Name = "Pendientes / lo que le falta al vehículo")]
        [StringLength(800, ErrorMessage = "Máximo {1} caracteres.")]
        public string? Pendientes { get; set; }

        [Display(Name = "Fotografías finales (opcional)")]
        public List<IFormFile>? FotosFinalesNuevas { get; set; }

        [Display(Name = "Confirmo que terminé la revisión y la envío al dueño para su autorización")]
        public bool Confirmacion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Confirmacion)
            {
                yield return new ValidationResult("Confirma que la revisión terminó.", new[] { nameof(Confirmacion) });
            }
        }
    }

    /// <summary>Regresar un vehículo a revisión (lo puede hacer el mecánico o el dueño) indicando el motivo.</summary>
    public class RegresarRevisionViewModel
    {
        public int VehiculoId { get; set; }

        [Display(Name = "Motivo")]
        [Required(ErrorMessage = "Indica el motivo para regresarlo a revisión.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre {2} y {1} caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>Subida de fotografías durante la revisión o finales.</summary>
    public class SubirFotosViewModel
    {
        public int VehiculoId { get; set; }
        public TipoFoto Tipo { get; set; }
        public List<IFormFile>? Archivos { get; set; }
    }

    // ───────────────────────── Documentación (asistente) ─────────────────────────

    /// <summary>Actualización de la documentación (CU-04: si queda pendiente, se exige indicar qué falta).</summary>
    public class DocumentacionViewModel : IValidatableObject
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public EstadoVehiculo EstadoOperativo { get; set; }

        [Display(Name = "Documento de propiedad / título")]
        public bool TieneTitulo { get; set; }

        [Display(Name = "Documento de importación / aduana")]
        public bool TieneImportacion { get; set; }

        [Display(Name = "Tarjeta de circulación / matrícula")]
        public bool TieneTarjetaCirculacion { get; set; }

        [Display(Name = "Documento de traspaso")]
        public bool TieneTraspaso { get; set; }

        [Display(Name = "Otros documentos")]
        public bool TieneOtros { get; set; }

        [Display(Name = "Estado documental")]
        public EstadoDocumentacion Estado { get; set; } = EstadoDocumentacion.Pendiente;

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
        public string? Observaciones { get; set; }

        public string? Origen { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Estado == EstadoDocumentacion.Pendiente && string.IsNullOrWhiteSpace(Observaciones))
            {
                yield return new ValidationResult(
                    "Si la documentación queda pendiente, indica qué documento falta.",
                    new[] { nameof(Observaciones) });
            }

            if (Estado == EstadoDocumentacion.Completa &&
                !(TieneTitulo && TieneImportacion && TieneTarjetaCirculacion && TieneTraspaso))
            {
                yield return new ValidationResult(
                    "Para marcarla como completa deben estar los cuatro documentos principales.",
                    new[] { nameof(Estado) });
            }
        }
    }

    /// <summary>Fila del módulo Documentación.</summary>
    public class DocumentacionFilaViewModel
    {
        public VehiculoListadoDto Vehiculo { get; set; } = null!;
        public Documentacion Documentacion { get; set; } = null!;
    }

    public class DocumentacionIndexViewModel
    {
        public string? Busqueda { get; set; }
        public EstadoDocumentacion? Filtro { get; set; }
        public int Total { get; set; }
        public int Pendientes { get; set; }
        public int Completas { get; set; }
        public List<DocumentacionFilaViewModel> Filas { get; set; } = new();
    }

    // ───────────────────────── Comercial (dueño) ─────────────────────────

    /// <summary>Registro de apartado (CU-05).</summary>
    public class ApartadoViewModel
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public string? Origen { get; set; }

        [Display(Name = "Nombre del cliente")]
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string NombreCliente { get; set; } = string.Empty;

        [Display(Name = "Teléfono / contacto")]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\+?[0-9]{4}-?[0-9]{4}$|^\+?[0-9 \-]{8,15}$", ErrorMessage = "Formato de teléfono no válido (ej. 7122-9034).")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Monto de anticipo (USD)")]
        [Required(ErrorMessage = "El anticipo es obligatorio.")]
        [Range(1, 1_000_000, ErrorMessage = "El anticipo debe ser mayor que cero.")]
        public decimal? Anticipo { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
        public string? Observaciones { get; set; }
    }

    /// <summary>Cancelación de un apartado (el vehículo vuelve a Disponible).</summary>
    public class CancelarApartadoViewModel
    {
        public int VehiculoId { get; set; }

        [Display(Name = "Motivo de la cancelación")]
        [Required(ErrorMessage = "Indica el motivo de la cancelación.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre {2} y {1} caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>Registro de venta (CU-06).</summary>
    public class VentaViewModel
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public bool VieneDeApartado { get; set; }
        public decimal? AnticipoPrevio { get; set; }
        public string? Origen { get; set; }

        [Display(Name = "Fecha de venta")]
        [Required(ErrorMessage = "La fecha de venta es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? FechaVenta { get; set; } = DateTime.Today;

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string NombreCliente { get; set; } = string.Empty;

        [Display(Name = "Teléfono / contacto")]
        [RegularExpression(@"^\+?[0-9]{4}-?[0-9]{4}$|^\+?[0-9 \-]{8,15}$", ErrorMessage = "Formato de teléfono no válido (ej. 7122-9034).")]
        public string? Telefono { get; set; }

        [Display(Name = "Monto de venta (USD, opcional)")]
        [Range(1, 10_000_000, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal? Monto { get; set; }
    }

    public class DisponiblesViewModel
    {
        public List<VehiculoListadoDto> Vehiculos { get; set; } = new();
        public ApartadoViewModel NuevoApartado { get; set; } = new();
        public bool PuedeApartar { get; set; }
    }

    public class ApartadosIndexViewModel
    {
        public List<ApartadoListadoDto> Apartados { get; set; } = new();
        public bool PuedeGestionar { get; set; }
    }

    /// <summary>Filtros y resultados del reporte de ventas.</summary>
    public class ReporteViewModel
    {
        [Display(Name = "Año")]
        public int Anio { get; set; } = DateTime.Today.Year;

        [Display(Name = "Mes")]
        [Range(1, 12)]
        public int? Mes { get; set; }

        [Display(Name = "Día")]
        [Range(1, 31)]
        public int? Dia { get; set; }

        [Display(Name = "Buscar vehículo")]
        public string? Busqueda { get; set; }

        public List<int> AniosDisponibles { get; set; } = new();
        public ReporteVentasDto Resultado { get; set; } = new();
    }

    // ───────────────────────── Bitácora y usuarios ─────────────────────────

    public class BitacoraViewModel
    {
        public string? Busqueda { get; set; }
        public List<MovimientoBitacora> Movimientos { get; set; } = new();
    }

    public class TrazabilidadViewModel
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public string? Origen { get; set; }
        public List<MovimientoBitacora> Movimientos { get; set; } = new();
    }

    public class UsuariosViewModel
    {
        public int UsuarioActualId { get; set; }
        public List<Usuario> Usuarios { get; set; } = new();
    }

    /// <summary>Datos que muestra el parcial _DatosCliente (estado comercial de la ficha).</summary>
    public class DatosClienteParcial
    {
        public Cliente Cliente { get; set; } = null!;
        public decimal? Anticipo { get; set; }
        public decimal? Monto { get; set; }
        public DateTime? Fecha { get; set; }
        public string EtiquetaFecha { get; set; } = "Fecha";
        public string? Observaciones { get; set; }
    }
}
