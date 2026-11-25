using System.Net;
using System.Net.Http.Json;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Tests
{
    /// <summary>Políticas de autorización, gestión de usuarios, documentación Swagger y respuestas de error.</summary>
    [Collection(ColeccionApi.Nombre)]
    public class SeguridadYDocumentacionTests
    {
        private readonly FabricaApi _api;

        public SeguridadYDocumentacionTests(FabricaApi api) => _api = api;

        [Theory]
        [InlineData(FabricaApi.Patio, "/api/usuarios")]
        [InlineData(FabricaApi.Mecanico, "/api/bitacora")]
        [InlineData(FabricaApi.Asistente, "/api/ventas")]
        [InlineData(FabricaApi.Patio, "/api/documentaciones")]
        public async Task RecursoDeOtroRol_Responde403(string correo, string ruta)
        {
            using var cliente = await _api.ClienteComoAsync(correo);

            var respuesta = await cliente.GetAsync(ruta);

            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task CrearUsuario_ComoDueno_Responde201YElNuevoUsuarioPuedeIniciarSesion()
        {
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);
            var nuevo = new CrearUsuarioDto { Nombre = "Lucía", Apellido = "Prueba", Rol = TipoRol.Mecanico, Contrasena = "Taller2026" };

            var respuesta = await dueno.PostAsJsonAsync("/api/usuarios", nuevo);

            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
            var creado = await respuesta.Content.ReadFromJsonAsync<UsuarioDto>(FabricaApi.Json);
            Assert.Equal("lucia.prueba@sedimports.com", creado!.Correo);

            using var anonimo = _api.CreateClient();
            var login = await anonimo.PostAsJsonAsync("/api/auth/login", new LoginDto { Correo = creado.Correo, Contrasena = "Taller2026" });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        [Fact]
        public async Task CrearUsuario_ContrasenaDebil_Responde400()
        {
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var respuesta = await dueno.PostAsJsonAsync("/api/usuarios",
                new CrearUsuarioDto { Nombre = "Pedro", Apellido = "Debil", Rol = TipoRol.Mecanico, Contrasena = "abc" });

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task Resumen_DevuelveLosIndicadoresDelPanel()
        {
            using var patio = await _api.ClienteComoAsync(FabricaApi.Patio);

            var resumen = await patio.GetFromJsonAsync<ResumenDto>("/api/reportes/resumen", FabricaApi.Json);

            Assert.True(resumen!.Vehiculos.Total > 0);
            Assert.Equal(0, resumen.VentasMes);   // El encargado de patio no recibe datos comerciales.
        }

        [Fact]
        public async Task Bitacora_DevuelveResultadosPaginados()
        {
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var pagina = await dueno.GetFromJsonAsync<PaginaDto<MovimientoDto>>("/api/bitacora?pagina=1&tamano=5", FabricaApi.Json);

            Assert.Equal(1, pagina!.Pagina);
            Assert.True(pagina.Elementos.Count <= 5);
            Assert.True(pagina.TotalElementos >= pagina.Elementos.Count);
        }

        [Fact]
        public async Task Swagger_PublicaLaDocumentacionDeLosEndpoints()
        {
            using var cliente = _api.CreateClient();

            var json = await cliente.GetStringAsync("/swagger/v1/swagger.json");

            Assert.Contains("/api/vehiculos", json);
            Assert.Contains("/api/auth/login", json);
            Assert.Contains("Bearer", json);
        }

        [Fact]
        public async Task Catalogos_DevuelvenLosEstadosDelVehiculo()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Patio);

            var estados = await cliente.GetFromJsonAsync<List<CatalogoDto>>("/api/catalogos/estados-vehiculo", FabricaApi.Json);

            Assert.Equal(6, estados!.Count);
            Assert.Contains(estados, e => e.Clave == "Disponible");
        }
    }
}
