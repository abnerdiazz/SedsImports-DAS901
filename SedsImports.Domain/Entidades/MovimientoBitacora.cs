using System.ComponentModel.DataAnnotations;
using SedsImports.Domain.Enums;

namespace SedsImports.Domain.Entidades
{
    /// <summary>
    /// Registro de cada cambio hecho en el sistema (tabla MovimientosBitacora): quién, cuándo,
    /// sobre qué vehículo y qué hizo. Alimenta la Bitácora general y la Trazabilidad de cada vehículo.
    /// Guarda también el nombre del usuario y del vehículo en ese momento, para que el historial
    /// no cambie aunque después se editen esos datos (registro de auditoría).
    /// </summary>
    public class MovimientoBitacora : IEntidad
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>Llave foránea (opcional) hacia Usuarios.</summary>
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [StringLength(80)]
        public string UsuarioNombre { get; set; } = string.Empty;

        [StringLength(60)]
        public string RolNombre { get; set; } = string.Empty;

        public TipoRol? RolTipo { get; set; }

        /// <summary>Llave foránea (opcional) hacia Vehiculos: los movimientos de usuarios no tienen vehículo.</summary>
        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [StringLength(80)]
        public string? VehiculoNombre { get; set; }

        public TipoMovimiento Tipo { get; set; }

        [StringLength(600)]
        public string Accion { get; set; } = string.Empty;
    }
}
