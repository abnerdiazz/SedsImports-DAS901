namespace SedsImports.Application.DTOs.Api
{
    /// <summary>Cliente que aparta o compra vehículos.</summary>
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}
