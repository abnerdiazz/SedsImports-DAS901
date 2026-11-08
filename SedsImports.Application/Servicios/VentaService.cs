using SedsImports.Application.Comun;
using SedsImports.Application.DTOs;
using SedsImports.Application.Interfaces;
using SedsImports.Application.ViewModels;
using SedsImports.Domain.Entidades;
using SedsImports.Domain.Enums;
using SedsImports.Domain.Extensiones;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>Ventas (RF-10, CU-06) y reporte mensual de ventas (RF-12, CU-08).</summary>
    public class VentaService : ServicioBase, IVentaService
    {
        private readonly IVentaRepository _ventas;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IApartadoRepository _apartados;
        private readonly IClienteService _clientes;
        private readonly IBitacoraService _bitacora;

        public VentaService(IUnidadDeTrabajo unidadDeTrabajo, IVentaRepository ventas, IVehiculoRepository vehiculos,
            IApartadoRepository apartados, IClienteService clientes, IBitacoraService bitacora)
            : base(unidadDeTrabajo)
        {
            _ventas = ventas;
            _vehiculos = vehiculos;
            _apartados = apartados;
            _clientes = clientes;
            _bitacora = bitacora;
        }

        public List<VentaDetalleDto> Listar() => _ventas.ObtenerConDetalle().Select(ADetalle).ToList();

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
                cliente = apartado.Cliente;
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
                Vehiculo = vehiculo,
                Cliente = cliente,
                Apartado = apartado,
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

            // Apartado, venta, cliente nuevo, vehículo y bitácora se confirman en una sola transacción.
            return Guardar($"Venta registrada: {vehiculo.NombreCompleto} vendido a {cliente.Nombre}.");
        }

        public ReporteVentasDto Reporte(int anio, int? mes, int? dia, string? busqueda)
        {
            var delAnio = _ventas.ObtenerConDetalle(anio).Select(ADetalle).ToList();
            var hayBusqueda = !string.IsNullOrWhiteSpace(busqueda);

            if (hayBusqueda)
            {
                var texto = busqueda!.Trim();
                delAnio = delAnio.Where(v => v.Vehiculo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                             || v.Cliente.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var reporte = new ReporteVentasDto { Anio = anio };

            // Totales por mes: sin búsqueda se agrupan en la base de datos (GROUP BY);
            // con búsqueda se agrupan las ventas ya filtradas.
            var porMes = hayBusqueda
                ? delAnio.GroupBy(v => v.Fecha.Month).ToDictionary(g => g.Key, g => (Cantidad: g.Count(), Total: g.Sum(v => v.Monto ?? 0)))
                : _ventas.ResumenMensual(anio).ToDictionary(r => r.Mes, r => (r.Cantidad, r.Total));

            // La gráfica siempre muestra los 12 meses del año (si no hay ventas, el valor es cero: CU-08).
            for (var m = 1; m <= 12; m++)
            {
                porMes.TryGetValue(m, out var datos);
                reporte.PorMes.Add(new VentaMensualDto
                {
                    Mes = m,
                    NombreMes = Formato.MesCorto(m),
                    Cantidad = datos.Cantidad,
                    Total = datos.Total
                });
            }

            var detalle = delAnio.AsEnumerable();
            if (mes.HasValue) detalle = detalle.Where(v => v.Fecha.Month == mes.Value);
            if (dia.HasValue) detalle = detalle.Where(v => v.Fecha.Day == dia.Value);
            reporte.Detalle = detalle.OrderByDescending(v => v.Fecha).ToList();

            reporte.PorMarca = _ventas.ResumenPorMarca(anio)
                .Select(r => new VentaMarcaDto
                {
                    Marca = r.Marca,
                    Cantidad = r.Cantidad,
                    Total = r.Total,
                    PromedioDiasEnPatio = r.PromedioDiasEnPatio
                })
                .ToList();

            return reporte;
        }

        public List<int> AniosConVentas()
        {
            var anios = _ventas.AniosConVentas().ToHashSet();
            anios.Add(DateTime.Today.Year);
            return anios.OrderByDescending(a => a).ToList();
        }

        public (int Cantidad, decimal Total) VentasDelMes(DateTime fecha) => _ventas.TotalesDelMes(fecha.Year, fecha.Month);

        private static VentaDetalleDto ADetalle(Venta v) => new()
        {
            VentaId = v.Id,
            VehiculoId = v.VehiculoId,
            Vehiculo = v.Vehiculo?.NombreCompleto ?? "—",
            Cliente = v.Cliente?.Nombre ?? "—",
            Telefono = v.Cliente?.Telefono,
            Fecha = v.FechaVenta,
            Monto = v.Monto
        };
    }
}
