using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SedsImports.Infrastructure.Datos;

namespace SedsImports.Infrastructure.Identidad
{
    /// <summary>
    /// Al iniciar: aplica las migraciones de Identity, crea los cuatro roles y una cuenta de acceso
    /// para cada usuario del negocio que todavía no la tenga (los usuarios de la Fase 3).
    /// </summary>
    public static class InicializadorIdentidad
    {
        public static async Task InicializarAsync(IServiceProvider servicios)
        {
            using var alcance = servicios.CreateScope();
            var proveedor = alcance.ServiceProvider;
            var contexto = proveedor.GetRequiredService<IdentidadDbContext>();
            var negocio = proveedor.GetRequiredService<SedsImportsDbContext>();
            var roles = proveedor.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var cuentas = proveedor.GetRequiredService<UserManager<UsuarioIdentidad>>();
            var log = proveedor.GetRequiredService<ILoggerFactory>().CreateLogger("SedsImports.Identidad");

            if (contexto.Database.IsSqlServer())
                await contexto.Database.MigrateAsync();
            else
                await CrearTablasAsync(contexto);

            // La API y la web se inician al mismo tiempo y las dos ejecutan este código. Si una crea un rol
            // o una cuenta un instante antes que la otra, la segunda recibe un error de llave duplicada:
            // se ignora, porque el dato ya existe, y se continúa con el siguiente.
            foreach (var nombre in RolesSistema.Todos)
            {
                if (await roles.RoleExistsAsync(nombre)) continue;
                try
                {
                    await roles.CreateAsync(new IdentityRole<int>(nombre));
                }
                catch (DbUpdateException)
                {
                    contexto.ChangeTracker.Clear();
                    log.LogInformation("El rol {Rol} ya fue creado por la otra aplicación.", nombre);
                }
            }

            // Usuarios de la Fase 3: se les crea su cuenta con la contraseña de demostración.
            // Luego cada uno puede tener la suya; Identity guarda solo el hash con sal.
            var usuarios = await negocio.Usuarios.Include(u => u.Rol).AsNoTracking().ToListAsync();
            var conCuenta = await contexto.Users.Select(c => c.UsuarioId).ToListAsync();

            foreach (var usuario in usuarios.Where(u => !conCuenta.Contains(u.Id)))
            {
                var cuenta = new UsuarioIdentidad
                {
                    UserName = usuario.Correo,
                    Email = usuario.Correo,
                    EmailConfirmed = true,
                    UsuarioId = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    LockoutEnabled = true,
                    LockoutEnd = usuario.Activo ? null : DateTimeOffset.MaxValue
                };

                try
                {
                    var creada = await cuentas.CreateAsync(cuenta, DatosIniciales.ContrasenaDemo);
                    if (creada.Succeeded)
                        await cuentas.AddToRoleAsync(cuenta, RolesSistema.De(usuario.Rol.Tipo));
                    else if (await cuentas.FindByEmailAsync(usuario.Correo) == null)
                        log.LogError("No se pudo crear la cuenta de {Correo}: {Errores}", usuario.Correo,
                            string.Join(" ", creada.Errors.Select(e => e.Description)));
                }
                catch (DbUpdateException)
                {
                    contexto.ChangeTracker.Clear();
                    log.LogInformation("La cuenta de {Correo} ya fue creada por la otra aplicación.", usuario.Correo);
                }
            }

            // Reparación: cada cuenta debe tener exactamente el rol de su usuario del negocio. Corrige cuentas
            // que quedaron sin rol (por ejemplo, si la aplicación se detuvo justo al crearlas) o con un rol viejo.
            foreach (var usuario in usuarios)
            {
                var cuenta = await cuentas.Users.FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id);
                if (cuenta == null) continue;

                var esperado = RolesSistema.De(usuario.Rol.Tipo);
                var actuales = await cuentas.GetRolesAsync(cuenta);
                if (actuales.Count == 1 && actuales[0] == esperado) continue;

                try
                {
                    if (actuales.Count > 0) await cuentas.RemoveFromRolesAsync(cuenta, actuales);
                    await cuentas.AddToRoleAsync(cuenta, esperado);
                    log.LogWarning("Se corrigió el rol de la cuenta {Correo}: {Rol}.", usuario.Correo, esperado);
                }
                catch (DbUpdateException)
                {
                    contexto.ChangeTracker.Clear();
                }
            }

            log.LogInformation("Identity listo: {Roles} roles y {Cuentas} cuentas.", RolesSistema.Todos.Length, await contexto.Users.CountAsync());
        }

        /// <summary>
        /// Con otros proveedores (SQLite en las pruebas automáticas) las tablas de Identity se agregan
        /// a la base que ya creó el contexto del negocio.
        /// </summary>
        private static async Task CrearTablasAsync(IdentidadDbContext contexto)
        {
            var creador = contexto.GetService<IRelationalDatabaseCreator>();
            try { await creador.CreateTablesAsync(); }
            catch (Exception) { /* Las tablas ya existen. */ }
        }
    }
}
