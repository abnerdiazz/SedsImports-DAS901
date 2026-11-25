using System.Net;
using System.Net.Http.Json;
using SedsImports.Application.DTOs.Api;
using SedsImports.Domain.Enums;

namespace SedsImports.Tests
{
    /// <summary>
    /// Flujo completo del negocio por la API, con un usuario distinto en cada paso:
    /// registro (patio) → revisión (mecánico) → autorización, apartado y venta (dueño).
    /// </summary>
    [Collection(ColeccionApi.Nombre)]
    public class FlujoCompletoTests
    {
        private readonly FabricaApi _api;

        public FlujoCompletoTests(FabricaApi api) => _api = api;

        [Fact]
        public async Task DelRegistroALaVenta_CadaPasoRespondeElCodigoCorrecto()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var mecanico = await _api.ClienteComoAsync(FabricaApi.Mecanico);
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);
            var ruta = $"/api/vehiculos/{vehiculo.Id}";

            // Una anotación antes de iniciar la revisión no se permite (regla del negocio → 409).
            var notaAntes = await mecanico.PostAsJsonAsync($"{ruta}/revision/notas", new CrearNotaDto { Texto = "Antes de iniciar" });
            Assert.Equal(HttpStatusCode.Conflict, notaAntes.StatusCode);

            // Revisión: iniciar (201), anotar (201) y finalizar (204).
            Assert.Equal(HttpStatusCode.Created, (await mecanico.PostAsync($"{ruta}/revision", null)).StatusCode);

            var nota = await mecanico.PostAsJsonAsync($"{ruta}/revision/notas", new CrearNotaDto { Texto = "Cambio de aceite y filtros." });
            Assert.Equal(HttpStatusCode.Created, nota.StatusCode);
            Assert.Equal("Cambio de aceite y filtros.", (await nota.Content.ReadFromJsonAsync<NotaDto>(FabricaApi.Json))!.Texto);

            var finalizar = await mecanico.PutAsJsonAsync($"{ruta}/revision/finalizacion",
                new FinalizarRevisionDto { ResumenFinal = "Mantenimiento general completo." });
            Assert.Equal(HttpStatusCode.NoContent, finalizar.StatusCode);

            // El mecánico no puede autorizar (403); el dueño sí (204).
            Assert.Equal(HttpStatusCode.Forbidden, (await mecanico.PostAsync($"{ruta}/autorizacion", null)).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await dueno.PostAsync($"{ruta}/autorizacion", null)).StatusCode);

            var disponible = await dueno.GetFromJsonAsync<VehiculoDetalleDto>(ruta, FabricaApi.Json);
            Assert.Equal(EstadoVehiculo.Disponible, disponible!.Estado);

            // Apartado (201) con un anticipo válido.
            var apartado = await dueno.PostAsJsonAsync("/api/apartados", new CrearApartadoDto
            {
                VehiculoId = vehiculo.Id, NombreCliente = "Cliente de Prueba", Telefono = "7777-8888", Anticipo = 500
            });
            Assert.Equal(HttpStatusCode.Created, apartado.StatusCode);

            // Venta (201): el vehículo queda vendido y ya no se puede editar (409).
            var venta = await dueno.PostAsJsonAsync("/api/ventas", new CrearVentaDto
            {
                VehiculoId = vehiculo.Id, FechaVenta = DateTime.Today, NombreCliente = "Cliente de Prueba", Telefono = "7777-8888", Monto = 12500
            });
            Assert.Equal(HttpStatusCode.Created, venta.StatusCode);

            var editar = await dueno.PutAsJsonAsync(ruta, new ActualizarVehiculoDto
            {
                Marca = "Toyota", Modelo = "Corolla", Anio = 2019, Color = "Negro", Vin = vehiculo.Vin
            });
            Assert.Equal(HttpStatusCode.Conflict, editar.StatusCode);

            // La bitácora registró cada paso del vehículo.
            var bitacora = await dueno.GetFromJsonAsync<List<MovimientoDto>>($"{ruta}/bitacora", FabricaApi.Json);
            Assert.True(bitacora!.Count >= 5);
        }

        [Fact]
        public async Task Apartado_ConAnticipoCero_Responde400()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var dueno = await _api.ClienteComoAsync(FabricaApi.Dueno);

            var respuesta = await dueno.PostAsJsonAsync("/api/apartados", new CrearApartadoDto
            {
                VehiculoId = vehiculo.Id, NombreCliente = "Cliente", Telefono = "7777-8888", Anticipo = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task Documentacion_CompletaSinLosCuatroDocumentos_Responde400()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var asistente = await _api.ClienteComoAsync(FabricaApi.Asistente);

            var respuesta = await asistente.PutAsJsonAsync($"/api/vehiculos/{vehiculo.Id}/documentacion", new ActualizarDocumentacionDto
            {
                TieneTitulo = true, Estado = EstadoDocumentacion.Completa
            });

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task Documentacion_ChecklistValido_Responde204()
        {
            var vehiculo = await Utilidades.RegistrarVehiculoAsync(_api);
            using var asistente = await _api.ClienteComoAsync(FabricaApi.Asistente);

            var respuesta = await asistente.PutAsJsonAsync($"/api/vehiculos/{vehiculo.Id}/documentacion", new ActualizarDocumentacionDto
            {
                TieneTitulo = true, TieneImportacion = true, Estado = EstadoDocumentacion.Pendiente, Observaciones = "Falta la tarjeta."
            });

            Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
            var doc = await asistente.GetFromJsonAsync<DocumentacionDto>($"/api/vehiculos/{vehiculo.Id}/documentacion", FabricaApi.Json);
            Assert.True(doc!.TieneTitulo);
            Assert.Equal("Falta la tarjeta.", doc.Observaciones);
        }
    }
}
