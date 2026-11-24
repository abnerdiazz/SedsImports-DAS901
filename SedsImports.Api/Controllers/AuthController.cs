using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Extensiones;

namespace SedsImports.Api.Controllers
{
    /// <summary>
    /// Autenticación: el cliente envía correo y contraseña, Identity los verifica y la API devuelve
    /// un token JWT firmado. Ese token se envía después en cada petición (Authorization: Bearer).
    /// </summary>
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IUsuarioService _usuarios;
        private readonly IServicioIdentidad _identidad;
        private readonly IGeneradorToken _tokens;

        public AuthController(IUsuarioService usuarios, IServicioIdentidad identidad, IGeneradorToken tokens)
        {
            _usuarios = usuarios;
            _identidad = identidad;
            _tokens = tokens;
        }

        /// <summary>Inicia sesión y devuelve el token JWT.</summary>
        /// <remarks>
        /// Usuarios de prueba (contraseña Seds2026): ernesto.rivas@sedimports.com (dueño),
        /// mario.sanchez@sedimports.com (patio), jose.hernandez@sedimports.com (mecánico),
        /// dania.martinez@sedimports.com (asistente). Tras 5 intentos fallidos la cuenta se bloquea 15 minutos.
        /// </remarks>
        /// <response code="200">Credenciales correctas: token, fecha de vencimiento y datos del usuario.</response>
        /// <response code="400">Faltan datos o el correo no es válido.</response>
        /// <response code="401">Correo o contraseña incorrectos, usuario desactivado o cuenta bloqueada.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var resultado = await _usuarios.AutenticarAsync(dto.Correo, dto.Contrasena);
            if (!resultado.Exitoso || resultado.Dato == null)
                return Problem(resultado.Mensaje, statusCode: StatusCodes.Status401Unauthorized, title: "No autenticado");

            var usuario = resultado.Dato;
            // Roles de la cuenta de Identity. El rol del usuario en el negocio siempre se incluye,
            // así el token coincide con los permisos que el usuario tiene en el sistema.
            var roles = (await _identidad.RolesAsync(usuario.Id)).ToList();
            var rolNegocio = usuario.Rol.Tipo.ToString();
            if (!roles.Contains(rolNegocio)) roles.Add(rolNegocio);

            var (token, expira) = _tokens.Generar(usuario, roles);

            return Ok(new TokenDto { Token = token, Expira = expira, Usuario = ADto(usuario) });
        }

        /// <summary>Datos del usuario dueño del token (sirve para comprobar que el token es válido).</summary>
        /// <response code="200">Usuario autenticado.</response>
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        public ActionResult<UsuarioDto> Perfil() => Ok(ADto(Actor));

        internal static UsuarioDto ADto(Usuario u) => new()
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Correo = u.Correo,
            Rol = u.Rol.Tipo,
            RolNombre = u.Rol.Tipo.NombreVisible(),
            Activo = u.Activo
        };
    }
}
