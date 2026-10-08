using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Filters;
using SedsImports.Web.Models;
using SedsImports.Web.Services;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Controllers
{
    /// <summary>Panel de inicio. Cada perfil ve un panel distinto con lo que necesita.</summary>
    public class HomeController : BaseController
    {
        private readonly IVehiculoService _vehiculos;
        private readonly IRevisionService _revisiones;
        private readonly IDocumentacionService _documentacion;
        private readonly IVentaService _ventas;
        private readonly IBitacoraService _bitacora;

        public HomeController(IVehiculoService vehiculos, IRevisionService revisiones,
            IDocumentacionService documentacion, IVentaService ventas, IBitacoraService bitacora)
        {
            _vehiculos = vehiculos;
            _revisiones = revisiones;
            _documentacion = documentacion;
            _ventas = ventas;
            _bitacora = bitacora;
        }

        [PerfilRequerido]
        public IActionResult Index()
        {
            var usuario = UsuarioActual;
            var hoy = DateTime.Today;
            var visibles = Permisos.EstadosVisibles(usuario.Rol.Tipo);
            var todos = _vehiculos.Listar(estadosVisibles: visibles);

            var modelo = new InicioViewModel
            {
                Usuario = usuario,
                Hoy = hoy,
                Conteos = _vehiculos.Conteos(visibles),
                RecienLlegados = todos.Where(v => v.Estado == EstadoVehiculo.RecienLlegado).ToList(),
                EnRevision = todos.Where(v => v.Estado == EstadoVehiculo.EnRevision).ToList(),
                PorAutorizar = todos.Where(v => v.Estado == EstadoVehiculo.RevisionFinalizada).ToList(),
                Disponibles = todos.Where(v => v.Estado == EstadoVehiculo.Disponible).ToList()
            };

            switch (usuario.Rol.Tipo)
            {
                case TipoRol.EncargadoPatio:
                    modelo.RegistradosHoy = _vehiculos.RegistradosDesde(hoy);
                    modelo.RegistradosSemana = _vehiculos.RegistradosDesde(hoy.AddDays(-6));
                    modelo.UltimosIngresos = todos.Take(6).ToList();
                    break;

                case TipoRol.Mecanico:
                    modelo.PreparacionTerminada = todos
                        .Where(v => v.Estado is EstadoVehiculo.RevisionFinalizada or EstadoVehiculo.Disponible)
                        .ToList();
                    modelo.MisNotas = _revisiones.NotasRecientes(usuario.Id, 5);
                    break;

                case TipoRol.AsistenteAdministrativa:
                case TipoRol.DuenoVendedor:
                    var (pendientes, completas) = _documentacion.Conteos();
                    modelo.DocumentacionPendiente = pendientes;
                    modelo.DocumentacionCompleta = completas;
                    modelo.ConDocumentacionPendiente = todos
                        .Where(v => v.Estado != EstadoVehiculo.Vendido && v.EstadoDocumentacion == EstadoDocumentacion.Pendiente)
                        .ToList();
                    var (vendidos, total) = _ventas.VentasDelMes(hoy);
                    modelo.VendidosMes = vendidos;
                    modelo.VentasMes = total;
                    modelo.ActividadReciente = _bitacora.Recientes(6);
                    break;
            }

            return View(modelo);
        }

        [Route("/Home/Error")]
        public IActionResult Error(int? codigo = null)
        {
            ViewData["Codigo"] = codigo ?? 500;
            return View();
        }
    }
}
