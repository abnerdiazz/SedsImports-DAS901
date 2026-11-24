using System.Net;
using System.Net.Http.Json;
using SedsImports.Application.DTOs;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Tests
{
    /// <summary>CRUD de vehículos: respuestas HTTP, validaciones y autorización por rol.</summary>
    [Collection(ColeccionApi.Nombre)]
    public class VehiculosApiTests
    {
        private readonly FabricaApi _api;

        public VehiculosApiTests(FabricaApi api) => _api = api;

        [Fact]
        public async Task Listar_ConToken_Devuelve200ConLosVehiculos()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var vehiculos = await cliente.GetFromJsonAsync<List<VehiculoListadoDto>>("/api/vehiculos", FabricaApi.Json);

            Assert.NotNull(vehiculos);
            Assert.NotEmpty(vehiculos!);
        }

        [Fact]
        public async Task Listar_ComoMecanico_SoloDevuelveLosEstadosQueLeCorresponden()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Mecanico);

            var vehiculos = await cliente.GetFromJsonAsync<List<VehiculoListadoDto>>("/api/vehiculos", FabricaApi.Json);

            Assert.DoesNotContain(vehiculos!, v => v.Estado is EstadoVehiculo.Apartado or EstadoVehiculo.Vendido);
        }

        [Fact]
        public async Task Obtener_VehiculoQueNoExiste_Responde404()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var respuesta = await cliente.GetAsync("/api/vehiculos/999999");

            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        }

        [Fact]
        public async Task Registrar_DatosValidos_Responde201ConLocation()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Patio);
            var dto = Utilidades.VehiculoValido();

            var respuesta = await cliente.PostAsJsonAsync("/api/vehiculos", dto);

            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
            Assert.NotNull(respuesta.Headers.Location);
            var creado = await respuesta.Content.ReadFromJsonAsync<VehiculoDetalleDto>(FabricaApi.Json);
            Assert.Equal(dto.Vin, creado!.Vin);
            Assert.Equal(EstadoVehiculo.RecienLlegado, creado.Estado);

            // El recurso creado se puede consultar en la dirección indicada.
            var consulta = await cliente.GetAsync(respuesta.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        }

        [Fact]
        public async Task Registrar_VinRepetido_Responde409()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Patio);
            var vin = Utilidades.VinNuevo();
            (await cliente.PostAsJsonAsync("/api/vehiculos", Utilidades.VehiculoValido(vin))).EnsureSuccessStatusCode();

            var respuesta = await cliente.PostAsJsonAsync("/api/vehiculos", Utilidades.VehiculoValido(vin));

            Assert.True(respuesta.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest,
                $"Se esperaba 409 o 400 y se obtuvo {(int)respuesta.StatusCode}.");
            Assert.Contains("VIN", await respuesta.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("", "Corolla", 2019, "VIN123456789012")]           // Falta la marca
        [InlineData("Toyota", "Corolla", 1800, "VIN123456789012")]     // Año fuera de rango
        [InlineData("Toyota", "Corolla", 2019, "VIN CON ESPACIOS")]    // VIN con espacios
        [InlineData("Toyota", "Corolla", 2019, "CORTO")]               // VIN muy corto
        public async Task Registrar_DatosNoValidos_Responde400(string marca, string modelo, int anio, string vin)
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Patio);
            var dto = new CrearVehiculoDto { Marca = marca, Modelo = modelo, Anio = anio, Color = "Rojo", Vin = vin };

            var respuesta = await cliente.PostAsJsonAsync("/api/vehiculos", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("errors", await respuesta.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Registrar_ComoMecanico_Responde403()
        {
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Mecanico);

            var respuesta = await cliente.PostAsJsonAsync("/api/vehiculos", Utilidades.VehiculoValido());

            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task Editar_DatosValidos_Responde204YGuardaLosCambios()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var cliente = await _api.ClienteComoAsync(FabricaApi.Patio);
            var cambios = new ActualizarVehiculoDto { Marca = "Toyota", Modelo = "Yaris", Anio = 2020, Color = "Azul", Vin = vehiculo.Vin };

            var respuesta = await cliente.PutAsJsonAsync($"/api/vehiculos/{vehiculo.Id}", cambios);

            Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
            var actualizado = await cliente.GetFromJsonAsync<VehiculoDetalleDto>($"/api/vehiculos/{vehiculo.Id}", FabricaApi.Json);
            Assert.Equal("Yaris", actualizado!.Modelo);
            Assert.Equal("Azul", actualizado.Color);
        }

        [Fact]
        public async Task Eliminar_ComoDueno_Responde204YLuegoNoSeEncuentra()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var respuesta = await dueno.DeleteAsync($"/api/vehiculos/{vehiculo.Id}");

            Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await dueno.GetAsync($"/api/vehiculos/{vehiculo.Id}")).StatusCode);
        }

        [Fact]
        public async Task Eliminar_ComoEncargadoDePatio_Responde403()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var patio = await _api.ClienteComoAsync(FabricaApi.Patio);

            var respuesta = await patio.DeleteAsync($"/api/vehiculos/{vehiculo.Id}");

            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }
    }
}
