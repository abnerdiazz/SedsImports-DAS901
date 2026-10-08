using Microsoft.AspNetCore.Mvc;
using SedsImports.Web.Models;
using SedsImports.Web.Services;

namespace SedsImports.Web.Controllers
{
    /// <summary>
    /// Métodos comunes a todos los controladores: usuario actual, mensajes al usuario
    /// (TempData → parcial _Mensajes) y paso de errores del servicio al ModelState.
    /// </summary>
    public abstract class BaseController : Controller
    {
        public const string ClaveExito = "MensajeExito";
        public const string ClaveError = "MensajeError";

        /// <summary>Usuario de la sesión. Las acciones protegidas con [PerfilRequerido] siempre lo tienen.</summary>
        protected Usuario UsuarioActual =>
            HttpContext.RequestServices.GetRequiredService<IUsuarioActualService>().Usuario
            ?? throw new InvalidOperationException("No hay sesión activa.");

        protected TipoRol RolActual => UsuarioActual.Rol.Tipo;

        protected void MostrarExito(string mensaje) => TempData[ClaveExito] = mensaje;

        protected void MostrarError(string mensaje) => TempData[ClaveError] = mensaje;

        /// <summary>Muestra el resultado de una operación como mensaje de éxito o de error.</summary>
        protected void MostrarResultado(ResultadoOperacion resultado)
        {
            if (string.IsNullOrWhiteSpace(resultado.Mensaje)) return;
            if (resultado.Exitoso) MostrarExito(resultado.Mensaje);
            else MostrarError(resultado.Mensaje);
        }

        /// <summary>Copia los errores del servicio al ModelState para que se vean junto a cada campo.</summary>
        protected void AgregarErrores(ResultadoOperacion resultado)
        {
            if (resultado.ErroresCampo.Count == 0)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return;
            }

            foreach (var error in resultado.ErroresCampo)
                ModelState.AddModelError(error.Key, error.Value);
        }

        /// <summary>Primer mensaje de error del ModelState (para formularios dentro de la ficha).</summary>
        protected string PrimerError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            ?? "Revisa los datos ingresados.";

        /// <summary>Regresa a la ficha del vehículo (o a la página de origen indicada).</summary>
        protected IActionResult VolverAFicha(int vehiculoId, string? ancla = null) =>
            Redirect(Url.Action("Detalle", "Vehiculos", new { id = vehiculoId }) + (ancla != null ? "#" + ancla : string.Empty));
    }
}
