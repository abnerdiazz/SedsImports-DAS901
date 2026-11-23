using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Extensiones;

namespace SedsImports.Application.Mapeos
{
    /// <summary>
    /// Conversión entre entidades, DTOs de la API y ViewModels de vehículos.
    /// Se hace con métodos de extensión escritos a mano (sin AutoMapper): el mapeo queda
    /// explícito, se revisa en tiempo de compilación y no agrega dependencias externas.
    /// </summary>
    public static class VehiculoMapeos
    {
        /// <summary>Entidad → ficha completa para la API. No expone las entidades relacionadas.</summary>
        public static VehiculoDetalleDto ADetalleDto(this Vehiculo v) => new()
        {
            Id = v.Id,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Anio = v.Anio,
            Color = v.Color,
            Vin = v.Vin,
            Detalles = v.Detalles,
            Estado = v.Estado,
            EstadoNombre = v.Estado.NombreVisible(),
            FechaIngreso = v.FechaIngreso,
            RegistradoPor = v.RegistradoPorNombre,
            FechaUltimaEdicion = v.FechaUltimaEdicion,
            EditadoPor = v.EditadoPorNombre,
            EstadoDocumentacion = v.Documentacion?.Estado,
            CantidadFotos = v.Fotografias.Count,
            CantidadNotas = v.Revision?.Notas.Count ?? 0
        };

        /// <summary>DTO de la API → ViewModel que reciben los servicios (mismas reglas que la Web).</summary>
        public static VehiculoFormViewModel AFormulario(this CrearVehiculoDto dto, int? id = null) => new()
        {
            Id = id,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Anio = dto.Anio,
            Color = dto.Color,
            Vin = dto.Vin,
            Detalles = dto.Detalles
        };

        /// <summary>Conteos por estado con el nombre del estado como clave (más fácil de leer en JSON).</summary>
        public static ConteosVehiculosDto ADto(this ConteoEstadosDto conteos) => new()
        {
            Total = conteos.Total,
            PorEstado = Enum.GetValues<EstadoVehiculo>().ToDictionary(e => e.ToString(), conteos.De)
        };

        /// <summary>Valores de cualquier enumeración → catálogo { valor, clave, nombre }.</summary>
        public static List<CatalogoDto> ACatalogo<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(e => new CatalogoDto { Valor = Convert.ToInt32(e), Clave = e.ToString(), Nombre = e.NombreVisible() })
                .ToList();
    }
}
