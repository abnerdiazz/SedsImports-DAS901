using SedsImports.Web.DTOs;
using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>
    /// Apartados (RF-09, CU-05). Al apartar, el vehículo deja de aparecer como disponible,
    /// lo que evita ofrecer a otro cliente un vehículo ya reservado.
    /// </summary>
    public class ApartadoService : IApartadoService
    {
        private readonly IApartadoRepository _apartados;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IDocumentacionRepository _documentos;
        private readonly IClienteService _clientes;
        private readonly IBitacoraService _bitacora;

        public ApartadoService(IApartadoRepository apartados, IVehiculoRepository vehiculos,
            IDocumentacionRepository documentos, IClienteService clientes, IBitacoraService bitacora)
        {
            _apartados = apartados;
            _vehiculos = vehiculos;
            _documentos = documentos;
            _clientes = clientes;
            _bitacora = bitacora;
        }

        public List<ApartadoListadoDto> ListarActivos() =>
            _apartados.ObtenerActivos()
                .OrderByDescending(a => a.FechaApartado)
                .Select(a =>
                {
                    var vehiculo = _vehiculos.ObtenerPorId(a.VehiculoId);
                    var cliente = _clientes.Obtener(a.ClienteId);
                    return new ApartadoListadoDto
                    {
                        ApartadoId = a.Id,
                        VehiculoId = a.VehiculoId,
                        Vehiculo = vehiculo?.NombreCompleto ?? "—",
                        Cliente = cliente?.Nombre ?? "—",
                        Contacto = cliente?.Telefono,
                        Anticipo = a.Anticipo,
                        Fecha = a.FechaApartado,
                        EstadoDocumentacion = _documentos.ObtenerPorVehiculo(a.VehiculoId)?.Estado ?? EstadoDocumentacion.Pendiente
                    };
                })
                .ToList();

        public Apartado? ObtenerActivo(int vehiculoId) => _apartados.ObtenerActivoPorVehiculo(vehiculoId);

        public ResultadoOperacion Registrar(ApartadoViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeGestionarComercial(actor.Rol.Tipo))
                return ResultadoOperacion.Error("Solo el dueño / vendedor puede registrar apartados.");

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            // CU-05, flujo alterno: si el vehículo ya no está disponible, no se permite apartar.
            if (vehiculo.Estado != EstadoVehiculo.Disponible)
                return ResultadoOperacion.Error($"{vehiculo.NombreCompleto} no está disponible para apartar (estado actual: {vehiculo.Estado.NombreVisible()}).");

            if (modelo.Anticipo is null or <= 0)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Anticipo), "El anticipo debe ser mayor que cero.");

            var cliente = _clientes.ObtenerOCrear(modelo.NombreCliente, modelo.Telefono);
            _apartados.Agregar(new Apartado
            {
                VehiculoId = vehiculo.Id,
                ClienteId = cliente.Id,
                FechaApartado = DateTime.Now,
                Anticipo = modelo.Anticipo.Value,
                Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones.Trim(),
                RegistradoPor = actor.NombreCompleto
            });

            vehiculo.Estado = EstadoVehiculo.Apartado;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Comercial,
                $"Registró apartado · Cliente: {cliente.Nombre} · Anticipo: {Formato.Dinero(modelo.Anticipo)}");

            return ResultadoOperacion.Ok($"{vehiculo.NombreCompleto} quedó apartado para {cliente.Nombre}.");
        }

        public ResultadoOperacion Cancelar(CancelarApartadoViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeGestionarComercial(actor.Rol.Tipo))
                return ResultadoOperacion.Error("Solo el dueño / vendedor puede cancelar apartados.");

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            var apartado = _apartados.ObtenerActivoPorVehiculo(modelo.VehiculoId);
            if (vehiculo == null || apartado == null || vehiculo.Estado != EstadoVehiculo.Apartado)
                return ResultadoOperacion.Error("El vehículo no tiene un apartado activo.");

            if (string.IsNullOrWhiteSpace(modelo.Motivo))
                return ResultadoOperacion.ErrorCampo(nameof(modelo.Motivo), "Indica el motivo de la cancelación.");

            apartado.Estado = EstadoApartado.Cancelado;
            apartado.FechaCierre = DateTime.Now;
            apartado.MotivoCancelacion = modelo.Motivo.Trim();
            _apartados.Actualizar(apartado);

            vehiculo.Estado = EstadoVehiculo.Disponible;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Comercial,
                $"Canceló apartado · Apartado → Disponible para venta · Motivo: {apartado.MotivoCancelacion}");

            return ResultadoOperacion.Ok($"Apartado cancelado. {vehiculo.NombreCompleto} vuelve a estar disponible.");
        }
    }
}
