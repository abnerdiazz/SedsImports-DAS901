using System.Net.Http.Json;
using SedsImports.Application.DTOs.Api;

namespace SedsImports.Tests
{
    /// <summary>Datos de prueba reutilizables.</summary>
    public static class Utilidades
    {
        private static int _consecutivo = 1000;

        /// <summary>VIN único de 17 caracteres para que cada prueba registre su propio vehículo.</summary>
        public static string VinNuevo() => $"PRUEBA{Interlocked.Increment(ref _consecutivo):D11}";

        public static CrearVehiculoDto VehiculoValido(string? vin = null) => new()
        {
            Marca = "Toyota",
            Modelo = "Corolla",
            Anio = 2019,
            Color = "Gris",
            Vin = vin ?? VinNuevo(),
            Detalles = "Registrado por las pruebas automáticas."
        };

        /// <summary>Registra un vehículo como encargado de patio y devuelve su ficha.</summary>
        public static async Task<VehiculoDetalleDto> RegistrarVehiculoAsync(FabricaApi api)
        {
            using var patio = await api.ClienteComoAsync(FabricaApi.Patio);
            var respuesta = await patio.PostAsJsonAsync("/api/vehiculos", VehiculoValido());
            respuesta.EnsureSuccessStatusCode();
            return (await respuesta.Content.ReadFromJsonAsync<VehiculoDetalleDto>(FabricaApi.Json))!;
        }
    }
}
