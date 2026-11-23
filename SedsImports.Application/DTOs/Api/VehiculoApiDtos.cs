using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.DTOs.Api
{
    /// <summary>
    /// Datos que recibe la API para registrar un vehículo (POST /api/vehiculos).
    /// Las validaciones se revisan en el servidor antes de llegar al servicio:
    /// si algo falla, la API responde 400 con el detalle de cada campo.
    /// </summary>
    public class CrearVehiculoDto
    {
        /// <example>Toyota</example>
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(40, ErrorMessage = "La marca admite máximo {1} caracteres.")]
        public string Marca { get; set; } = string.Empty;

        /// <example>Corolla</example>
        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(40, ErrorMessage = "El modelo admite máximo {1} caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        /// <example>2019</example>
        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(1950, 2100, ErrorMessage = "Ingresa un año entre {1} y {2}.")]
        public int? Anio { get; set; }

        /// <example>Gris</example>
        [Required(ErrorMessage = "El color es obligatorio.")]
        [StringLength(30, ErrorMessage = "El color admite máximo {1} caracteres.")]
        public string Color { get; set; } = string.Empty;

        /// <example>2T1BURHE5KC123456</example>
        [Required(ErrorMessage = "El VIN es obligatorio.")]
        [StringLength(17, MinimumLength = 11, ErrorMessage = "El VIN debe tener entre {2} y {1} caracteres.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "El VIN solo admite letras y números, sin espacios.")]
        public string Vin { get; set; } = string.Empty;

        /// <example>Golpe leve en la puerta trasera derecha.</example>
        [StringLength(500, ErrorMessage = "Los detalles admiten máximo {1} caracteres.")]
        public string? Detalles { get; set; }
    }

    /// <summary>Datos para editar un vehículo (PUT /api/vehiculos/{id}). Mismas reglas que el registro.</summary>
    public class ActualizarVehiculoDto : CrearVehiculoDto
    {
    }

    /// <summary>Ficha completa de un vehículo que devuelve GET /api/vehiculos/{id}.</summary>
    public class VehiculoDetalleDto
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string? Detalles { get; set; }
        public EstadoVehiculo Estado { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string RegistradoPor { get; set; } = string.Empty;
        public DateTime? FechaUltimaEdicion { get; set; }
        public string? EditadoPor { get; set; }
        public EstadoDocumentacion? EstadoDocumentacion { get; set; }
        public int CantidadFotos { get; set; }
        public int CantidadNotas { get; set; }
    }

    /// <summary>Respuesta de GET /api/vehiculos/conteos: total y cantidad por estado.</summary>
    public class ConteosVehiculosDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> PorEstado { get; set; } = new();
    }

    /// <summary>Elemento de un catálogo (estados, roles, tipos de movimiento) para listas desplegables.</summary>
    public class CatalogoDto
    {
        public int Valor { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
