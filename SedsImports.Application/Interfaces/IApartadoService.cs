using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;

namespace SedsImports.Application.Interfaces
{
    public interface IApartadoService
    {
        List<ApartadoListadoDto> ListarActivos();
        Apartado? ObtenerActivo(int vehiculoId);
        ResultadoOperacion Registrar(ApartadoViewModel modelo, Usuario actor);
        ResultadoOperacion Cancelar(CancelarApartadoViewModel modelo, Usuario actor);
    }
}
