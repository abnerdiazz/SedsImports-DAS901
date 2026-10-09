using Microsoft.AspNetCore.Html;

namespace SedsImports.Web.Helpers
{
    /// <summary>Íconos SVG en línea (no dependen de internet ni de librerías externas).</summary>
    public static class Iconos
    {
        private const string Abre = "<svg class=\"icono\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">";

        private static readonly Dictionary<string, string> Trazos = new()
        {
            ["inicio"] = "<path d=\"M3 10.5 12 3l9 7.5\"/><path d=\"M5 9.5V21h14V9.5\"/>",
            ["vehiculos"] = "<path d=\"M5 16h14\"/><path d=\"M4 16v-4l2-5h12l2 5v4\"/><circle cx=\"7.5\" cy=\"16.5\" r=\"1.5\"/><circle cx=\"16.5\" cy=\"16.5\" r=\"1.5\"/><path d=\"M4 12h16\"/>",
            ["registrar"] = "<path d=\"M12 5v14M5 12h14\"/>",
            ["disponibles"] = "<path d=\"M3 15h18\"/><path d=\"M5 15l2-6h10l2 6\"/><circle cx=\"7.5\" cy=\"17.5\" r=\"1.5\"/><circle cx=\"16.5\" cy=\"17.5\" r=\"1.5\"/>",
            ["documento"] = "<path d=\"M14 3H6v18h12V7z\"/><path d=\"M14 3v4h4\"/><path d=\"M9 13h6M9 17h6\"/>",
            ["reportes"] = "<path d=\"M4 20h16\"/><path d=\"M7 16v-5M12 16V6M17 16v-8\"/>",
            ["bitacora"] = "<path d=\"M9 6h11M9 12h11M9 18h11\"/><circle cx=\"4.5\" cy=\"6\" r=\"1\"/><circle cx=\"4.5\" cy=\"12\" r=\"1\"/><circle cx=\"4.5\" cy=\"18\" r=\"1\"/>",
            ["usuarios"] = "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21c0-4 4-6 8-6s8 2 8 6\"/>",
            ["salir"] = "<path d=\"M14 4h5v16h-5\"/><path d=\"M10 8l-4 4 4 4\"/><path d=\"M6 12h10\"/>",
            ["menu"] = "<path d=\"M4 6h16M4 12h16M4 18h16\"/>",
            ["check"] = "<path d=\"M5 12l5 5 9-10\"/>",
            ["editar"] = "<path d=\"M4 20h4L19 9l-4-4L4 16z\"/>",
            ["auto"] = "<path d=\"M3 15h18\"/><path d=\"M5 15l2.5-6h9L19 15\"/><circle cx=\"7.5\" cy=\"17.5\" r=\"1.8\"/><circle cx=\"16.5\" cy=\"17.5\" r=\"1.8\"/>",
            ["camara"] = "<path d=\"M4 8h3l2-3h6l2 3h3v11H4z\"/><circle cx=\"12\" cy=\"13\" r=\"3.5\"/>",
            ["alerta"] = "<path d=\"M12 3l10 18H2z\"/><path d=\"M12 10v5M12 18h.01\"/>",
            ["flecha"] = "<path d=\"M15 6l-6 6 6 6\"/>"
        };

        public static IHtmlContent Svg(string nombre) =>
            new HtmlString(Abre + (Trazos.TryGetValue(nombre, out var t) ? t : string.Empty) + "</svg>");
    }
}
