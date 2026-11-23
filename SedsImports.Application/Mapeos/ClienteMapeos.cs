using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Mapeos
{
    /// <summary>Conversión de clientes a DTO.</summary>
    public static class ClienteMapeos
    {
        public static ClienteDto ADto(this Cliente c) => new()
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            Correo = c.Correo
        };
    }
}
