using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SedsImports.Application.Comun;
using SedsImports.Application.Interfaces;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Servicios
{
    /// <summary>
    /// Guarda las fotografías en wwwroot/uploads/vehiculos/{id} y registra cada una en la tabla
    /// Fotografias (a través de la colección del vehículo). El archivo va al disco y la ruta a la base de datos.
    /// </summary>
    public class FotoService : IFotoService
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximo = 5 * 1024 * 1024;
        private const int MaximoPorCarga = 10;

        private readonly IWebHostEnvironment _entorno;

        public FotoService(IWebHostEnvironment entorno) => _entorno = entorno;

        public async Task<ResultadoOperacion<int>> GuardarAsync(Vehiculo vehiculo, IEnumerable<IFormFile>? archivos, TipoFoto tipo, Usuario actor)
        {
            var lista = archivos?.Where(a => a is { Length: > 0 }).ToList() ?? new List<IFormFile>();
            if (lista.Count == 0) return ResultadoOperacion<int>.Ok(0, string.Empty);

            if (lista.Count > MaximoPorCarga)
                return ResultadoOperacion<int>.Error($"Puedes subir como máximo {MaximoPorCarga} fotografías a la vez.");

            var carpetaRelativa = Path.Combine("uploads", "vehiculos", vehiculo.Id.ToString());
            var carpeta = Path.Combine(_entorno.WebRootPath, carpetaRelativa);
            Directory.CreateDirectory(carpeta);

            var guardadas = 0;
            var omitidas = 0;
            foreach (var archivo in lista)
            {
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension) || archivo.Length > TamanoMaximo)
                {
                    omitidas++;
                    continue;
                }

                var nombre = $"{tipo.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}{extension}";
                await using (var destino = File.Create(Path.Combine(carpeta, nombre)))
                {
                    await archivo.CopyToAsync(destino);
                }

                // Al agregarla a la colección, EF Core la inserta con el siguiente GuardarCambios.
                vehiculo.Fotografias.Add(new Fotografia
                {
                    Tipo = tipo,
                    Ruta = "/" + carpetaRelativa.Replace('\\', '/') + "/" + nombre,
                    Descripcion = Recortar(Path.GetFileNameWithoutExtension(archivo.FileName), 120),
                    Fecha = DateTime.Now,
                    SubidaPor = actor.NombreCompleto
                });
                guardadas++;
            }

            return omitidas == 0
                ? ResultadoOperacion<int>.Ok(guardadas, $"{guardadas} fotografía(s) agregada(s).")
                : ResultadoOperacion<int>.Error($"Se omitieron {omitidas} archivo(s): solo se aceptan JPG, PNG o WEBP de hasta 5 MB.");
        }

        /// <summary>Evita que un nombre de archivo muy largo supere el tamaño de la columna.</summary>
        private static string Recortar(string texto, int maximo) => texto.Length <= maximo ? texto : texto[..maximo];
    }
}
