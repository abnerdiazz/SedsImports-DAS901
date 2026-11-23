using SedsImports.Application.DTOs.Api;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Mapeos
{
    /// <summary>Conversión de revisiones y anotaciones entre entidades, DTOs y ViewModels.</summary>
    public static class RevisionMapeos
    {
        public static RevisionDto ADto(this Revision r) => new()
        {
            Id = r.Id,
            VehiculoId = r.VehiculoId,
            Mecanico = r.MecanicoNombre,
            FechaInicio = r.FechaInicio,
            FechaFinalizacion = r.FechaFinalizacion,
            ResumenFinal = r.ResumenFinal,
            Pendientes = r.Pendientes,
            FechaAutorizacion = r.FechaAutorizacion,
            AutorizadoPor = r.AutorizadoPor,
            Notas = r.Notas.OrderByDescending(n => n.Fecha).Select(n => n.ADto()).ToList()
        };

        public static NotaDto ADto(this NotaRevision n) => new()
        {
            Id = n.Id,
            Fecha = n.Fecha,
            Usuario = n.UsuarioNombre,
            Texto = n.Texto,
            EsDelSistema = n.EsDelSistema
        };

        public static NotaRevisionViewModel AViewModel(this CrearNotaDto dto, int vehiculoId) =>
            new() { VehiculoId = vehiculoId, Texto = dto.Texto };

        public static FinalizarRevisionViewModel AViewModel(this FinalizarRevisionDto dto, int vehiculoId) =>
            new() { VehiculoId = vehiculoId, ResumenFinal = dto.ResumenFinal, Pendientes = dto.Pendientes, Confirmacion = true };

        public static RegresarRevisionViewModel AViewModel(this RegresarRevisionDto dto, int vehiculoId) =>
            new() { VehiculoId = vehiculoId, Motivo = dto.Motivo };
    }
}
