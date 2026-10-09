namespace SedsImports.Web.Models
{
    /// <summary>
    /// Registro de cada cambio hecho en el sistema: quién, cuándo, sobre qué vehículo y qué hizo.
    /// Alimenta la Bitácora general y la Trazabilidad de cada vehículo.
    /// </summary>
    public class MovimientoBitacora : IEntidad
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string RolNombre { get; set; } = string.Empty;
        public TipoRol? RolTipo { get; set; }

        public int? VehiculoId { get; set; }
        public string? VehiculoNombre { get; set; }

        public TipoMovimiento Tipo { get; set; }
        public string Accion { get; set; } = string.Empty;
    }
}
