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
    /// <summary>
    /// Apartados (RF-09, CU-05). Al apartar, el vehículo deja de aparecer como disponible,
    /// lo que evita ofrecer a otro cliente un vehículo ya reservado.
    /// </summary>
    public class ApartadoService : ServicioBase, IApartadoService
    {
        private readonly IApartadoRepository _apartados;
        private readonly IVehiculoRepository _vehiculos;
        private readonly IClienteService _clientes;
        private readonly IBitacoraService _bitacora;

        public ApartadoService(IUnidadDeTrabajo unidadDeTrabajo, IApartadoRepository apartados,
            IVehiculoRepository vehiculos, IClienteService clientes, IBitacoraService bitacora)
            : base(unidadDeTrabajo)
        {
            _apartados = apartados;
            _vehiculos = vehiculos;
            _clientes = clientes;
            _bitacora = bitacora;
        }

        /// <summary>Apartados activos con su vehículo, su cliente y su documentación (una sola consulta con joins).</summary>
        public List<ApartadoListadoDto> ListarActivos() =>
            _apartados.ObtenerActivos()
                .Select(a => new ApartadoListadoDto
                {
                    ApartadoId = a.Id,
                    VehiculoId = a.VehiculoId,
                    Vehiculo = a.Vehiculo.NombreCompleto,
                    Cliente = a.Cliente.Nombre,
                    Contacto = a.Cliente.Telefono,
                    Anticipo = a.Anticipo,
                    Fecha = a.FechaApartado,
                    EstadoDocumentacion = a.Vehiculo.Documentacion?.Estado ?? EstadoDocumentacion.Pendiente
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

            // Se enlazan las entidades (no los Id): si el cliente es nuevo, EF Core lo inserta
            // primero y usa su Id generado para el apartado, todo en el mismo GuardarCambios.
            _apartados.Agregar(new Apartado
            {
                Vehiculo = vehiculo,
                Cliente = cliente,
                FechaApartado = DateTime.Now,
                Anticipo = modelo.Anticipo.Value,
                Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones.Trim(),
                RegistradoPor = actor.NombreCompleto
            });

            vehiculo.Estado = EstadoVehiculo.Apartado;
            _vehiculos.Actualizar(vehiculo);

            _bitacora.Registrar(actor, vehiculo, TipoMovimiento.Comercial,
                $"Registró apartado · Cliente: {cliente.Nombre} · Anticipo: {Formato.Dinero(modelo.Anticipo)}");

            return Guardar($"{vehiculo.NombreCompleto} quedó apartado para {cliente.Nombre}.");
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

            return Guardar($"Apartado cancelado. {vehiculo.NombreCompleto} vuelve a estar disponible.");
        }
    }
}
