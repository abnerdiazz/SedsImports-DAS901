using SedsImports.Domain.Enums;

namespace SedsImports.Application.Comun
{
    /// <summary>
    /// Reglas de "quién puede hacer qué". Se usan en los servicios (validación de negocio)
    /// y en las vistas (mostrar u ocultar botones), para que ambas cosas coincidan siempre.
    /// </summary>
    public static class Permisos
    {
        public static bool PuedeRegistrarVehiculo(TipoRol rol) => rol is TipoRol.EncargadoPatio or TipoRol.DuenoVendedor;
        public static bool PuedeEditarVehiculo(TipoRol rol) => rol is TipoRol.EncargadoPatio or TipoRol.DuenoVendedor;
        public static bool PuedeRevisar(TipoRol rol) => rol == TipoRol.Mecanico;
        public static bool PuedeRegresarARevision(TipoRol rol) => rol is TipoRol.Mecanico or TipoRol.DuenoVendedor;
        public static bool PuedeAutorizar(TipoRol rol) => rol == TipoRol.DuenoVendedor;
        public static bool PuedeDocumentar(TipoRol rol) => rol is TipoRol.AsistenteAdministrativa or TipoRol.DuenoVendedor;
        public static bool PuedeGestionarComercial(TipoRol rol) => rol == TipoRol.DuenoVendedor;
        public static bool PuedeAdministrar(TipoRol rol) => rol == TipoRol.DuenoVendedor;
        public static bool PuedeEliminarVehiculo(TipoRol rol) => rol == TipoRol.DuenoVendedor;

        /// <summary>El mecánico solo ve los vehículos que le toca trabajar o que ya preparó.</summary>
        public static EstadoVehiculo[] EstadosVisibles(TipoRol rol) => rol == TipoRol.Mecanico
            ? new[] { EstadoVehiculo.RecienLlegado, EstadoVehiculo.EnRevision, EstadoVehiculo.RevisionFinalizada, EstadoVehiculo.Disponible }
            : Enum.GetValues<EstadoVehiculo>();

        public const string SinPermiso = "Tu perfil no tiene permiso para realizar esta acción.";
    }
}
