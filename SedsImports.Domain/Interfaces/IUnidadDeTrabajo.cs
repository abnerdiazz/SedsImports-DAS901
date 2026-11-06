namespace SedsImports.Domain.Interfaces
{
    /// <summary>
    /// Unidad de Trabajo: confirma en la base de datos, de una sola vez, todos los cambios
    /// que hicieron los repositorios durante una operación. Si algo falla, no se guarda nada.
    /// </summary>
    public interface IUnidadDeTrabajo
    {
        /// <summary>Guarda todos los cambios pendientes. Lanza ErrorPersistenciaException si la base de datos los rechaza.</summary>
        int GuardarCambios();

        /// <summary>Inicia una transacción para operaciones que necesitan guardar en varios pasos.</summary>
        ITransaccion IniciarTransaccion();
    }

    /// <summary>Transacción abierta. Si no se confirma, al cerrarse se revierte.</summary>
    public interface ITransaccion : IDisposable
    {
        void Confirmar();
        void Revertir();
    }
}
