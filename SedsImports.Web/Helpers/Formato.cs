using System.Globalization;

namespace SedsImports.Web.Helpers
{
    /// <summary>Formatos de fecha y dinero usados en todas las vistas (consistencia visual).</summary>
    public static class Formato
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-SV");
        private static readonly CultureInfo Moneda = CultureInfo.GetCultureInfo("en-US");

        public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Cultura);

        public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy · HH:mm", Cultura);

        public static string Dinero(decimal? monto) =>
            monto.HasValue ? "$" + monto.Value.ToString("#,##0.##", Moneda) : "—";

        public static string NombreMes(int mes) =>
            Cultura.DateTimeFormat.GetMonthName(mes) is { Length: > 0 } nombre
                ? char.ToUpper(nombre[0]) + nombre[1..]
                : mes.ToString();

        public static string MesCorto(int mes) => NombreMes(mes)[..3];
    }
}
