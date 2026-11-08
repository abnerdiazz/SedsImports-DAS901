using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SedsImports.Domain.Excepciones;
using SedsImports.Domain.Interfaces;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Repositorios
{
    /// <summary>
    /// Unidad de Trabajo: todos los repositorios de una petición comparten el mismo DbContext,
    /// así que un solo SaveChanges confirma todos sus cambios en una transacción.
    /// Los errores de la base de datos se traducen a ErrorPersistenciaException para que
    /// las capas superiores no dependan de Entity Framework.
    /// </summary>
    public class UnidadDeTrabajo : IUnidadDeTrabajo
    {
        private readonly SedsImportsDbContext _contexto;

        public UnidadDeTrabajo(SedsImportsDbContext contexto) => _contexto = contexto;

        public int GuardarCambios()
        {
            try
            {
                return _contexto.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ErrorPersistenciaException(
                    "Otro usuario modificó este registro al mismo tiempo. Recarga la página e inténtalo de nuevo.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new ErrorPersistenciaException(Traducir(ex), ex);
            }
        }

        public ITransaccion IniciarTransaccion() => new Transaccion(_contexto.Database.BeginTransaction());

        /// <summary>Mensaje entendible para el usuario según el error de SQL Server.</summary>
        private static string Traducir(DbUpdateException ex)
        {
            var detalle = ex.InnerException?.Message ?? ex.Message;

            if (detalle.Contains("IX_Vehiculos_Vin", StringComparison.OrdinalIgnoreCase))
                return "Ya existe un vehículo con este VIN.";
            if (detalle.Contains("IX_Usuarios_Correo", StringComparison.OrdinalIgnoreCase))
                return "Ya existe un usuario con este correo.";
            if (detalle.Contains("IX_Ventas_VehiculoId", StringComparison.OrdinalIgnoreCase))
                return "Este vehículo ya tiene una venta registrada.";
            if (detalle.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
                return "El registro ya existe. Verifica los datos.";
            if (detalle.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
                return "La operación no se pudo completar porque el registro está relacionado con otros datos.";
            if (detalle.Contains("CHECK", StringComparison.OrdinalIgnoreCase))
                return "Alguno de los valores no es válido.";

            return "No se pudieron guardar los cambios en la base de datos. Inténtalo de nuevo.";
        }

        /// <summary>Envoltura de la transacción de EF Core para no exponerla fuera de la infraestructura.</summary>
        private sealed class Transaccion : ITransaccion
        {
            private readonly IDbContextTransaction _transaccion;

            public Transaccion(IDbContextTransaction transaccion) => _transaccion = transaccion;

            public void Confirmar() => _transaccion.Commit();

            public void Revertir() => _transaccion.Rollback();

            public void Dispose() => _transaccion.Dispose();
        }
    }
}
