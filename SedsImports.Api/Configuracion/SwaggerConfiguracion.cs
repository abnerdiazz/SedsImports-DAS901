using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SedsImports.Api.Configuracion
{
    /// <summary>
    /// Documentación interactiva de la API con Swagger (OpenAPI). Toma los comentarios XML de los
    /// controladores y DTOs, y agrega el botón "Authorize" para probar los endpoints con el token JWT.
    /// </summary>
    public static class SwaggerConfiguracion
    {
        public static IServiceCollection AgregarDocumentacionApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(opciones =>
            {
                opciones.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SED's Imports & Services · API",
                    Version = "v1",
                    Description = "API REST del sistema de gestión y seguimiento vehicular. " +
                                  "1) Inicia sesión en POST /api/auth/login. 2) Copia el token. " +
                                  "3) Presiona Authorize y pégalo. Todas las respuestas de error usan ProblemDetails (RFC 7807).",
                    Contact = new OpenApiContact { Name = "Grupo 12 · DAS901 G01T", Url = new Uri("https://github.com/abnerdiazz/SedsImports-DAS901") }
                });

                // Botón "Authorize": el token se envía como Authorization: Bearer {token}.
                var esquema = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Pega solo el token JWT (sin la palabra Bearer).",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };
                opciones.AddSecurityDefinition("Bearer", esquema);
                opciones.AddSecurityRequirement(new OpenApiSecurityRequirement { { esquema, Array.Empty<string>() } });

                // Comentarios XML (///) de la API y de la capa de aplicación (DTOs con ejemplos).
                foreach (var ensamblado in new[] { Assembly.GetExecutingAssembly().GetName().Name, "SedsImports.Application" })
                {
                    var archivo = Path.Combine(AppContext.BaseDirectory, $"{ensamblado}.xml");
                    if (File.Exists(archivo)) opciones.IncludeXmlComments(archivo);
                }

                // Los enums se muestran como texto (RecienLlegado, Disponible...) igual que en el JSON.
                opciones.UseInlineDefinitionsForEnums();
            });

            return services;
        }

        public static WebApplication UsarDocumentacionApi(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opciones =>
            {
                opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "SED's Imports API v1");
                opciones.DocumentTitle = "SED's Imports · API";
                opciones.DisplayRequestDuration();
            });
            return app;
        }
    }
}
