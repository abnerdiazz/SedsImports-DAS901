using System.Net;
using System.Net.Http.Json;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Tests
{
    /// <summary>Inicio de sesión con ASP.NET Core Identity y protección de los endpoints con JWT.</summary>
    [Collection(ColeccionApi.Nombre)]
    public class AutenticacionTests
    {
        private readonly FabricaApi _api;

        public AutenticacionTests(FabricaApi api) => _api = api;

        [Fact]
        public async Task Login_ConCredencialesCorrectas_DevuelveTokenYUsuario()
        {
            using var cliente = _api.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Correo = FabricaApi.Asistente, Contrasena = FabricaApi.Contrasena });

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            var token = await respuesta.Content.ReadFromJsonAsync<TokenDto>(FabricaApi.Json);
            Assert.False(string.IsNullOrWhiteSpace(token!.Token));
            Assert.Equal("Bearer", token.Tipo);
            Assert.Equal(TipoRol.AsistenteAdministrativa, token.Usuario.Rol);
            Assert.True(token.Expira > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_ConContrasenaIncorrecta_Responde401SinRevelarCualFallo()
        {
            using var cliente = _api.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Correo = FabricaApi.Patio, Contrasena = "ContrasenaIncorrecta1" });

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
            var texto = await respuesta.Content.ReadAsStringAsync();
            Assert.Contains("Usuario o contraseña incorrectos", texto);
        }

        [Fact]
        public async Task Login_ConCorreoNoValido_Responde400ConErroresDeValidacion()
        {
            using var cliente = _api.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto { Correo = "no-es-correo", Contrasena = "" });

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            var texto = await respuesta.Content.ReadAsStringAsync();
            Assert.Contains("Correo", texto);
            Assert.Contains("Contrasena", texto);
        }

        [Fact]
        public async Task EndpointProtegido_SinToken_Responde401()
        {
            using var cliente = _api.CreateClient();

            var respuesta = await cliente.GetAsync("/api/vehiculos");

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
            Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task EndpointProtegido_ConTokenAlterado_Responde401()
        {
            using var cliente = _api.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", "eyJhbGciOiJIUzI1NiJ9.e30.firma-falsa");

            var respuesta = await cliente.GetAsync("/api/auth/perfil");

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [Fact]
        public async Task Perfil_ConToken_DevuelveElUsuarioDelToken()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var usuario = await cliente.GetFromJsonAsync<UsuarioDto>("/api/auth/perfil", FabricaApi.Json);

            Assert.Equal(FabricaApi.Dueno, usuario!.Correo);
            Assert.Equal(TipoRol.DuenoVendedor, usuario.Rol);
        }
    }
}
