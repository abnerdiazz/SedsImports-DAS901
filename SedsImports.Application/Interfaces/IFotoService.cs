using Microsoft.AspNetCore.Http;
using SedsImports.Application.Comun;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IFotoService
    {
        /// <summary>Guarda los archivos en disco y los agrega al vehículo. No guarda en la base de datos: lo hace quien lo llama.</summary>
        Task<ResultadoOperacion<int>> GuardarAsync(Vehiculo vehiculo, IEnumerable<IFormFile>? archivos, TipoFoto tipo, Usuario actor);
    }
}
