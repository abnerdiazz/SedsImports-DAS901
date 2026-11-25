using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.DTOs.Api
{
    // ───────────────────────── Documentación ─────────────────────────

    /// <summary>Expediente de documentos de un vehículo.</summary>
    public class DocumentacionDto
    {
        public int VehiculoId { get; set; }
        public string Vehiculo { get; set; } = string.Empty;
        public bool TieneTitulo { get; set; }
        public bool TieneImportacion { get; set; }
        public bool TieneTarjetaCirculacion { get; set; }
        public bool TieneTraspaso { get; set; }
        public bool TieneOtros { get; set; }
        public EstadoDocumentacion Estado { get; set; }
        public string? Observaciones { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? ActualizadoPor { get; set; }
    }

    /// <summary>Actualización del checklist (PUT /api/vehiculos/{id}/documentacion).</summary>
    public class ActualizarDocumentacionDto
    {
        public bool TieneTitulo { get; set; }
        public bool TieneImportacion { get; set; }
        public bool TieneTarjetaCirculacion { get; set; }
        public bool TieneTraspaso { get; set; }
        public bool TieneOtros { get; set; }

        /// <example>Pendiente</example>
        [Required(ErrorMessage = "Indica el estado de la documentación.")]
        [EnumDataType(typeof(EstadoDocumentacion), ErrorMessage = "Estado de documentación no válido.")]
        public EstadoDocumentacion? Estado { get; set; }

        /// <example>Falta la tarjeta de circulación.</example>
        [StringLength(500, ErrorMessage = "Las observaciones admiten máximo {1} caracteres.")]
        public string? Observaciones { get; set; }
    }

    // ───────────────────────── Apartados ─────────────────────────

    /// <summary>Nuevo apartado (POST /api/apartados).</summary>
    public class CrearApartadoDto
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Indica el vehículo.")]
        public int VehiculoId { get; set; }

        /// <example>María López</example>
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string NombreCliente { get; set; } = string.Empty;

        /// <example>7777-8888</example>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9+\-\s]{8,15}$", ErrorMessage = "Escribe un teléfono válido (8 a 15 dígitos).")]
        public string Telefono { get; set; } = string.Empty;

        /// <example>500</example>
        [Required(ErrorMessage = "El anticipo es obligatorio.")]
        [Range(typeof(decimal), "0.01", "999999", ErrorMessage = "El anticipo debe ser mayor que cero.")]
        public decimal? Anticipo { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones admiten máximo {1} caracteres.")]
        public string? Observaciones { get; set; }
    }

    /// <summary>Cancelación de un apartado activo.</summary>
    public class CancelarApartadoDto
    {
        /// <example>El cliente desistió de la compra.</example>
        [Required(ErrorMessage = "Indica el motivo de la cancelación.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre {2} y {1} caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>Apartado de un vehículo (activo, convertido en venta o cancelado).</summary>
    public class ApartadoDto
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public ClienteDto Cliente { get; set; } = new();
        public DateTime Fecha { get; set; }
        public decimal Anticipo { get; set; }
        public string? Observaciones { get; set; }
        public EstadoApartado Estado { get; set; }
        public string RegistradoPor { get; set; } = string.Empty;
    }

    // ───────────────────────── Ventas ─────────────────────────

    /// <summary>Nueva venta (POST /api/ventas). Si el vehículo estaba apartado, se convierte el apartado.</summary>
    public class CrearVentaDto
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Indica el vehículo.")]
        public int VehiculoId { get; set; }

        /// <example>2026-11-20</example>
        [Required(ErrorMessage = "La fecha de venta es obligatoria.")]
        public DateTime? FechaVenta { get; set; }

        /// <example>María López</example>
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        public string NombreCliente { get; set; } = string.Empty;

        /// <example>7777-8888</example>
        [RegularExpression(@"^[0-9+\-\s]{8,15}$", ErrorMessage = "Escribe un teléfono válido (8 a 15 dígitos).")]
        public string? Telefono { get; set; }

        /// <example>12500</example>
        [Range(typeof(decimal), "0", "9999999", ErrorMessage = "El monto no puede ser negativo.")]
        public decimal? Monto { get; set; }
    }

    // ───────────────────────── Bitácora y reportes ─────────────────────────

    /// <summary>Movimiento de la bitácora de auditoría.</summary>
    public class MovimientoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? VehiculoId { get; set; }
        public string? Vehiculo { get; set; }
        public TipoMovimiento Tipo { get; set; }
        public string Accion { get; set; } = string.Empty;
    }

    /// <summary>Resultado paginado: los elementos de la página y los datos para navegar.</summary>
    public class PaginaDto<T>
    {
        public List<T> Elementos { get; set; } = new();
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalElementos { get; set; }
        public int TotalPaginas => TamanoPagina == 0 ? 0 : (int)Math.Ceiling(TotalElementos / (double)TamanoPagina);
    }

    /// <summary>Indicadores del panel de inicio (GET /api/reportes/resumen).</summary>
    public class ResumenDto
    {
        public DateTime GeneradoEn { get; set; } = DateTime.Now;
        public ConteosVehiculosDto Vehiculos { get; set; } = new();
        public int DocumentacionPendiente { get; set; }
        public int DocumentacionCompleta { get; set; }
        public int ApartadosActivos { get; set; }
        public int VendidosMes { get; set; }
        public decimal VentasMes { get; set; }
        public int RegistradosHoy { get; set; }
    }
}
