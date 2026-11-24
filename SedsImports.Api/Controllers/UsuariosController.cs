using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedsImports.Api.Seguridad;
using SedsImports.Application.DTOs.Api;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;

namespace SedsImports.Api.Controllers
{
    /// <summary>Gestión de usuarios y roles. Solo el dueño / vendedor (administrador).</summary>
    [Route("api/usuarios")]
    [Authorize(Policy = Politicas.SoloDueno)]
    public class UsuariosController : ApiControllerBase
    {
        private readonly IUsuarioService _usuarios;
        private readonly IServicioIdentidad _identidad;

        public UsuariosController(IUsuarioService usuarios, IServicioIdentidad identidad)
        {
            _usuarios = usuarios;
            _identidad = identidad;
        }

        /// <summary>Lista los usuarios del sistema.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<UsuarioDto>> Listar() =>
            Ok(_usuarios.Listar().Select(AuthController.ADto).ToList());

        /// <summary>Datos de un usuario.</summary>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">El usuario no existe.</response>
        [HttpGet("{id:int}", Name = nameof(ObtenerUsuario))]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UsuarioDto> ObtenerUsuario(int id)
        {
            var usuario = _usuarios.Obtener(id);
            return usuario == null ? NoEncontrado("El usuario no existe.") : Ok(AuthController.ADto(usuario));
        }

        /// <summary>Roles de Identity asignados a un usuario.</summary>
        [HttpGet("{id:int}/roles")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<string>>> Roles(int id) => Ok(await _identidad.RolesAsync(id));

        /// <summary>Crea un usuario con su cuenta de acceso y su rol.</summary>
        /// <remarks>El correo se genera con el primer nombre y el primer apellido (lucia.mena@sedimports.com).</remarks>
        /// <response code="201">Usuario creado.</response>
        /// <response code="400">Datos no válidos o contraseña que no cumple la política.</response>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            var modelo = new NuevoUsuarioViewModel
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Rol = dto.Rol,
                Contrasena = dto.Contrasena,
                ConfirmarContrasena = dto.Contrasena,
                Activo = dto.Activo
            };

            var resultado = await _usuarios.CrearAsync(modelo, Actor);
            return Responder(resultado, () =>
                CreatedAtRoute(nameof(ObtenerUsuario), new { id = resultado.Dato!.Id }, AuthController.ADto(resultado.Dato!)));
        }

        /// <summary>Activa o desactiva un usuario (al desactivarlo se bloquea su cuenta de acceso).</summary>
        /// <response code="204">Estado cambiado.</response>
        /// <response code="404">El usuario no existe.</response>
        /// <response code="409">No puedes desactivar tu propio usuario.</response>
        [HttpPut("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CambiarEstado(int id) =>
            Responder(await _usuarios.CambiarEstadoAsync(id, Actor), NoContent);
    }
}
