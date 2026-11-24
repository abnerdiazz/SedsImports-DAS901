using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace SedsImports.Api.Seguridad
{
    /// <summary>Registro de la autenticación con JWT y de las políticas de autorización por rol.</summary>
    public static class ConfiguracionSeguridad
    {
        public static IServiceCollection AgregarSeguridadJwt(this IServiceCollection services, IConfiguration configuracion)
        {
            // Opciones validadas al iniciar: si falta la clave o es corta, la API no arranca.
            services.AddOptions<JwtOpciones>()
                .Bind(configuracion.GetSection(JwtOpciones.Seccion))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var jwt = configuracion.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>() ?? new JwtOpciones();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones =>
                {
                    opciones.MapInboundClaims = false;
                    opciones.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt.Emisor,
                        ValidAudience = jwt.Audiencia,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave.PadRight(32))),
                        ClockSkew = TimeSpan.FromMinutes(1),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.Name
                    };

                    // Respuestas 401/403 en formato ProblemDetails (JSON), igual que el resto de errores.
                    opciones.Events = new JwtBearerEvents
                    {
                        OnChallenge = async contexto =>
                        {
                            contexto.HandleResponse();
                            await EscribirProblema(contexto.HttpContext, StatusCodes.Status401Unauthorized, "No autenticado",
                                "Inicia sesión en /api/auth/login y envía el token en el encabezado Authorization: Bearer {token}.");
                        },
                        OnForbidden = contexto => EscribirProblema(contexto.HttpContext, StatusCodes.Status403Forbidden, "Acceso denegado",
                            "Tu rol no tiene permiso para usar este recurso.")
                    };
                });

            services.AddAuthorization(opciones =>
            {
                // Cada política exige uno de los roles definidos en Politicas.Roles.
                foreach (var (nombre, roles) in Politicas.Roles)
                    opciones.AddPolicy(nombre, p => p.RequireAuthenticatedUser().RequireRole(roles.Select(r => r.ToString())));
            });

            services.AddScoped<IGeneradorToken, GeneradorToken>();
            return services;
        }

        private static Task EscribirProblema(HttpContext http, int codigo, string titulo, string detalle)
        {
            http.Response.StatusCode = codigo;
            return http.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = codigo,
                Title = titulo,
                Detail = detalle,
                Instance = http.Request.Path
            }, options: null, contentType: "application/problem+json");
        }
    }
}
