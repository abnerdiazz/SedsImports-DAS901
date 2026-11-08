using SedsImports.Application.Comun;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    public interface IRevisionService
    {
        Revision? ObtenerPorVehiculo(int vehiculoId);
        ResultadoOperacion Iniciar(int vehiculoId, Usuario actor);
        ResultadoOperacion AgregarNota(NotaRevisionViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> FinalizarAsync(FinalizarRevisionViewModel modelo, Usuario actor);
        ResultadoOperacion RegresarARevision(RegresarRevisionViewModel modelo, Usuario actor);
        Task<ResultadoOperacion> SubirFotosAsync(SubirFotosViewModel modelo, Usuario actor);
        List<NotaResumen> NotasRecientes(int usuarioId, int cantidad);
    }
}
