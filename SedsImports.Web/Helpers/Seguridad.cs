using System.Security.Cryptography;
using System.Text;

namespace SedsImports.Web.Helpers
{
    /// <summary>
    /// Hash simple de contraseñas para el prototipo. En la Fase 4 (seguridad)
    /// se reemplaza por ASP.NET Core Identity o un hash con sal (PBKDF2).
    /// </summary>
    public static class Seguridad
    {
        public static string Hash(string texto)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }
    }
}
