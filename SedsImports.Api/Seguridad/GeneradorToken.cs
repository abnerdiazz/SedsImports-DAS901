using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SedsImports.Domain.Entidades;

namespace SedsImports.Api.Seguridad
{
    /// <summary>Crea el token JWT firmado de un usuario autenticado.</summary>
    public interface IGeneradorToken
    {
        (string Token, DateTime Expira) Generar(Usuario usuario, IEnumerable<string> roles);
    }

    public class GeneradorToken : IGeneradorToken
    {
        private readonly JwtOpciones _opciones;

        public GeneradorToken(IOptions<JwtOpciones> opciones) => _opciones = opciones.Value;

        public (string Token, DateTime Expira) Generar(Usuario usuario, IEnumerable<string> roles)
        {
            var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

            // Datos que viajan en el token. No se incluye información sensible (ni contraseñas ni hashes).
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Correo),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimsSeds.UsuarioId, usuario.Id.ToString()),
                new(ClaimsSeds.NombreCompleto, usuario.NombreCompleto),
                new(ClaimTypes.Name, usuario.Correo)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opciones.Emisor,
                audience: _opciones.Audiencia,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expira,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}
