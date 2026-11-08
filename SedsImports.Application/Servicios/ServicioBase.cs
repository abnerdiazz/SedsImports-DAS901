using SedsImports.Application.Comun;
using SedsImports.Domain.Excepciones;
using SedsImports.Domain.Interfaces;

namespace SedsImports.Application.Servicios
{
    /// <summary>
    /// Base de los servicios que modifican datos. Centraliza el guardado con la Unidad de Trabajo
    /// y convierte los errores de la base de datos en un ResultadoOperacion con un mensaje claro,
    /// para que el usuario nunca vea una pantalla de error por un problema al guardar.
    /// </summary>
    public abstract class ServicioBase
    {
        protected readonly IUnidadDeTrabajo UnidadDeTrabajo;

        protected ServicioBase(IUnidadDeTrabajo unidadDeTrabajo) => UnidadDeTrabajo = unidadDeTrabajo;

        /// <summary>Confirma los cambios pendientes. Si la base de datos los rechaza, devuelve el error.</summary>
        protected ResultadoOperacion Guardar(string mensajeExito)
        {
            try
            {
                UnidadDeTrabajo.GuardarCambios();
                return ResultadoOperacion.Ok(mensajeExito);
            }
            catch (ErrorPersistenciaException ex)
            {
                return ResultadoOperacion.Error(ex.Message);
            }
        }

        /// <summary>Igual que <see cref="Guardar"/>, pero devuelve además un dato (por ejemplo, la entidad creada).</summary>
        protected ResultadoOperacion<T> Guardar<T>(T dato, string mensajeExito)
        {
            try
            {
                UnidadDeTrabajo.GuardarCambios();
                return ResultadoOperacion<T>.Ok(dato, mensajeExito);
            }
            catch (ErrorPersistenciaException ex)
            {
                return ResultadoOperacion<T>.Error(ex.Message);
            }
        }
    }
}
