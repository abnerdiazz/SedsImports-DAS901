using SedsImports.Web.DTOs;
using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;
using SedsImports.Web.ViewModels;

namespace SedsImports.Web.Services
{
    /// <summary>Ventas (RF-10, CU-06) y reporte mensual de ventas (RF-12, CU-08).</summary>
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventas;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IApartadoRepository _apartados;
        private readonly IClienteService _clientes;
        private readonly IBitacoraService _bitacora;

        public VentaService(IVentaRepository ventas, IVehiculoRepository vehiculos, IApartadoRepository apartados,
            IClienteService clientes, IBitacoraService bitacora)
        {
            _ventas = ventas;
            _vehiculos = vehiculos;
            _apartados = apartados;
            _clientes = clientes;
            _bitacora = bitacora;
        }

        public List<VentaDetalleDto> Listar() =>
            _ventas.ObtenerTodos()
                .OrderByDescending(v => v.FechaVenta).ThenByDescending(v => v.FechaRegistro)
                .Select(ADetalle)
                .ToList();

        public Venta? ObtenerPorVehiculo(int vehiculoId) => _ventas.ObtenerPorVehiculo(vehiculoId);

        public ResultadoOperacion Registrar(VentaViewModel modelo, Usuario actor)
        {
            if (!Permisos.PuedeGestionarComercial(actor.Rol.Tipo))
                return ResultadoOperacion.Error("Solo el dueño / vendedor puede registrar ventas.");

            var vehiculo = _vehiculos.ObtenerPorId(modelo.VehiculoId);
            if (vehiculo == null) return ResultadoOperacion.Error("El vehículo no existe.");

            // CU-06, flujo alterno: no se puede vender dos veces el mismo vehículo.
            if (vehiculo.Estado == EstadoVehiculo.Vendido || _ventas.ObtenerPorVehiculo(vehiculo.Id) != null)
                return ResultadoOperacion.Error("Este vehículo ya figura como vendido.");

            if (vehiculo.Estado is not (EstadoVehiculo.Apartado or EstadoVehiculo.Disponible))
                return ResultadoOperacion.Error("Solo se puede vender un vehículo disponible o apartado.");

            if (modelo.FechaVenta is null)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.FechaVenta), "La fecha de venta es obligatoria.");
            if (modelo.FechaVenta.Value.Date > DateTime.Today)
                return ResultadoOperacion.ErrorCampo(nameof(modelo.FechaVenta), "La fecha de venta no puede ser futura.");

            var apartado = _apartados.ObtenerActivoPorVehiculo(vehiculo.Id);
            Cliente cliente;
            if (apartado != null)
            {
                // Si estaba apartado, la venta se registra a nombre del cliente del apartado.
                cliente = _clientes.Obtener(apartado.ClienteId) ?? _clientes.ObtenerOCrear(modelo.NombreCliente, modelo.Telefono);
                apartado.Estado = EstadoApartado.ConvertidoEnVenta;
                apartado.FechaCierre = DateTime.Now;
                _apartados.Actualizar(apartado);
            }
            else
            {
                cliente = _clientes.ObtenerOCrear(modelo.NombreCliente, modelo.Telefono);
            }

            _ventas.Agregar(new Venta
            {
                VehiculoId = vehiculo.Id,
                ClienteId = cliente.Id,
                ApartadoId = apartado?.Id,
                FechaVenta = modelo.FechaVenta.Value.Date,
                Monto = modelo.Monto,
                RegistradoPor = actor.NombreCompleto,
                FechaRegistro = DateTime.Now
            });

            var estadoAnterior = vehiculo.Estado;
            vehiculo.Estado = EstadoVehiculo.Vendido;
            _vehiculos.Actualizar(vehiculo);

            var monto = modelo.Monto.HasValue ? $" · Monto: {Formato.Dinero(modelo.Monto)}" : string.Empty;
            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Comercial,
                $"Registró venta · {estadoAnterior.NombreVisible()} → Vendido · Cliente: {cliente.Nombre}{monto}");

            return ResultadoOperacion.Ok($"Venta registrada: {vehiculo.NombreCompleto} vendido a {cliente.Nombre}.");
        }

        public ReporteVentasDto Reporte(int anio, int? mes, int? dia, string? busqueda)
        {
            var delAnio = _ventas.ObtenerTodos().Where(v => v.FechaVenta.Year == anio).Select(ADetalle).ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                delAnio = delAnio.Where(v => v.Vehiculo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                             || v.Cliente.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var reporte = new ReporteVentasDto { Anio = anio };

            // La gráfica siempre muestra los 12 meses del año (si no hay ventas, el valor es cero: CU-08).
            for (var m = 1; m <= 12; m++)
            {
                var delMes = delAnio.Where(v => v.Fecha.Month == m).ToList();
                reporte.PorMes.Add(new VentaMensualDto
                {
                    Mes = m,
                    NombreMes = Formato.MesCorto(m),
                    Cantidad = delMes.Count,
                    Total = delMes.Sum(v => v.Monto ?? 0)
                });
            }

            var detalle = delAnio.AsEnumerable();
            if (mes.HasValue) detalle = detalle.Where(v => v.Fecha.Month == mes.Value);
            if (dia.HasValue) detalle = detalle.Where(v => v.Fecha.Day == dia.Value);
            reporte.Detalle = detalle.OrderByDescending(v => v.Fecha).ToList();

            return reporte;
        }

        public List<int> AniosConVentas()
        {
            var anios = _ventas.ObtenerTodos().Select(v => v.FechaVenta.Year).ToHashSet();
            anios.Add(DateTime.Today.Year);
            return anios.OrderByDescending(a => a).ToList();
        }

        public (int Cantidad, decimal Total) VentasDelMes(DateTime fecha)
        {
            var delMes = _ventas.ObtenerTodos()
                .Where(v => v.FechaVenta.Year == fecha.Year && v.FechaVenta.Month == fecha.Month).ToList();
            return (delMes.Count, delMes.Sum(v => v.Monto ?? 0));
        }

        private VentaDetalleDto ADetalle(Venta v)
        {
            var vehiculo = _vehiculos.ObtenerPorId(v.VehiculoId);
            var cliente = _clientes.Obtener(v.ClienteId);
            return new VentaDetalleDto
            {
                VentaId = v.Id,
                VehiculoId = v.VehiculoId,
                Vehiculo = vehiculo?.NombreCompleto ?? "—",
                Cliente = cliente?.Nombre ?? "—",
                Telefono = cliente?.Telefono,
                Fecha = v.FechaVenta,
                Monto = v.Monto
            };
        }
    }
}
