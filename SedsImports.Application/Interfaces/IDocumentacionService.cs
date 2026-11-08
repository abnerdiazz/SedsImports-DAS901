using SedsImports.Application.Comun;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;

namespace SedsImports.Application.Interfaces
{
    public interface IDocumentacionService
    {
        Documentacion ObtenerPorVehiculo(int vehiculoId);
        DocumentacionIndexViewModel Listar(string? busqueda, EstadoDocumentacion? filtro);
        DocumentacionViewModel ArmarFormulario(int vehiculoId);
        ResultadoOperacion Actualizar(DocumentacionViewModel modelo, Usuario actor);
        (int Pendientes, int Completas) Conteos();
    }
}
