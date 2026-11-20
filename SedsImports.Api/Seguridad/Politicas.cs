using SedsImports.Domain.Enums;

namespace SedsImports.Api.Seguridad
{
    /// <summary>
    /// Políticas de autorización de la API. Cada política agrupa los roles que pueden usar un
    /// conjunto de endpoints; los controladores solo indican el nombre de la política con
    /// [Authorize(Policy = ...)]. Coinciden con las reglas de Permisos de la capa de aplicación.
    /// </summary>
    public static class Politicas
    {
        /// <summary>Registrar y editar vehículos: encargado de patio y dueño.</summary>
        public const string GestionPatio = nameof(GestionPatio);

        /// <summary>Iniciar la revisión, anotar y finalizar: mecánico.</summary>
        public const string Taller = nameof(Taller);

        /// <summary>Regresar un vehículo a revisión: mecánico y dueño.</summary>
        public const string TallerODueno = nameof(TallerODueno);

        /// <summary>Expediente de documentos: asistente administrativa y dueño.</summary>
        public const string Documentacion = nameof(Documentacion);

        /// <summary>Autorizar, eliminar, apartados, ventas, usuarios y bitácora: solo el dueño / vendedor.</summary>
        public const string SoloDueno = nameof(SoloDueno);

        /// <summary>Roles permitidos en cada política.</summary>
        public static readonly IReadOnlyDictionary<string, TipoRol[]> Roles = new Dictionary<string, TipoRol[]>
        {
            [GestionPatio] = new[] { TipoRol.EncargadoPatio, TipoRol.DuenoVendedor },
            [Taller] = new[] { TipoRol.Mecanico },
            [TallerODueno] = new[] { TipoRol.Mecanico, TipoRol.DuenoVendedor },
            [Documentacion] = new[] { TipoRol.AsistenteAdministrativa, TipoRol.DuenoVendedor },
            [SoloDueno] = new[] { TipoRol.DuenoVendedor }
        };
    }
}
