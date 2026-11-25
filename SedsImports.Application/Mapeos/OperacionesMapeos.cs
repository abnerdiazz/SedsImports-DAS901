using SedsImports.Application.DTOs.Api;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Mapeos
{
    /// <summary>Conversión de documentación, apartados, ventas y bitácora entre entidades, DTOs y ViewModels.</summary>
    public static class OperacionesMapeos
    {
        public static DocumentacionDto ADto(this Documentacion d, string vehiculo) => new()
        {
            VehiculoId = d.VehiculoId,
            Vehiculo = vehiculo,
            TieneTitulo = d.TieneTitulo,
            TieneImportacion = d.TieneImportacion,
            TieneTarjetaCirculacion = d.TieneTarjetaCirculacion,
            TieneTraspaso = d.TieneTraspaso,
            TieneOtros = d.TieneOtros,
            Estado = d.Estado,
            Observaciones = d.Observaciones,
            FechaActualizacion = d.FechaActualizacion,
            ActualizadoPor = d.ActualizadoPor
        };

        public static DocumentacionViewModel AViewModel(this ActualizarDocumentacionDto dto, int vehiculoId) => new()
        {
            VehiculoId = vehiculoId,
            TieneTitulo = dto.TieneTitulo,
            TieneImportacion = dto.TieneImportacion,
            TieneTarjetaCirculacion = dto.TieneTarjetaCirculacion,
            TieneTraspaso = dto.TieneTraspaso,
            TieneOtros = dto.TieneOtros,
            Estado = dto.Estado ?? Domain.Enums.EstadoDocumentacion.Pendiente,
            Observaciones = dto.Observaciones
        };

        public static ApartadoViewModel AViewModel(this CrearApartadoDto dto) => new()
        {
            VehiculoId = dto.VehiculoId,
            NombreCliente = dto.NombreCliente,
            Telefono = dto.Telefono,
            Anticipo = dto.Anticipo,
            Observaciones = dto.Observaciones
        };

        public static CancelarApartadoViewModel AViewModel(this CancelarApartadoDto dto, int vehiculoId) =>
            new() { VehiculoId = vehiculoId, Motivo = dto.Motivo };

        public static ApartadoDto ADto(this Apartado a) => new()
        {
            Id = a.Id,
            VehiculoId = a.VehiculoId,
            Cliente = a.Cliente?.ADto() ?? new ClienteDto { Id = a.ClienteId },
            Fecha = a.FechaApartado,
            Anticipo = a.Anticipo,
            Observaciones = a.Observaciones,
            Estado = a.Estado,
            RegistradoPor = a.RegistradoPor
        };

        public static VentaViewModel AViewModel(this CrearVentaDto dto) => new()
        {
            VehiculoId = dto.VehiculoId,
            FechaVenta = dto.FechaVenta,
            NombreCliente = dto.NombreCliente,
            Telefono = dto.Telefono,
            Monto = dto.Monto
        };

        public static MovimientoDto ADto(this MovimientoBitacora m) => new()
        {
            Id = m.Id,
            Fecha = m.Fecha,
            Usuario = m.UsuarioNombre,
            Rol = m.RolNombre,
            VehiculoId = m.VehiculoId,
            Vehiculo = m.VehiculoNombre,
            Tipo = m.Tipo,
            Accion = m.Accion
        };
    }
}
