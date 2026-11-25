# Despliegue en IIS — SED's Imports & Services

La solución se publica como **dos sitios de IIS** en la misma computadora:

| Sitio | Carpeta | Dirección |
|---|---|---|
| `SedsImports.Api` | `C:\inetpub\SedsImports\Api` | http://localhost:8081 (Swagger en `/swagger`, salud en `/health`) |
| `SedsImports.Web` | `C:\inetpub\SedsImports\Web` | http://localhost:8080 |

La web llama a la API en `http://localhost:8081/` (`SedsImports.Web/appsettings.Production.json`) y la API solo acepta llamadas de navegador desde `http://localhost:8080` (CORS).

## 1. Preparar Windows (una sola vez)

1. **Activar IIS:** *Panel de control → Programas → Activar o desactivar las características de Windows* → marcar **Internet Information Services** (con *Herramientas de administración web → Consola de administración de IIS*) → Aceptar.
2. **Instalar el ASP.NET Core Hosting Bundle de .NET 10:** <https://dotnet.microsoft.com/download/dotnet/10.0> → sección *ASP.NET Core Runtime* → **Hosting Bundle**. Después de instalarlo, reiniciar IIS: `iisreset` (CMD como administrador).
3. **SQL Server LocalDB** ya está instalado con Visual Studio. Comprobar: `sqllocaldb info MSSQLLocalDB`.

## 2. Publicar con el script (recomendado)

En **PowerShell como administrador**, desde la carpeta del repositorio:

```
powershell -ExecutionPolicy Bypass -File .\scripts\publicar-iis.ps1
```

El script:

1. Publica los dos proyectos en modo **Release** (`dotnet publish`).
2. Crea los grupos de aplicaciones **sin código administrado** y con el perfil de usuario cargado.
3. Los ejecuta con la cuenta del sistema (**LocalSystem**). LocalDB crea para esa cuenta su propia base `SedsImportsDB` con los datos de ejemplo (la primera vez tarda 1 o 2 minutos). Funciona también con cuentas Microsoft y no pide contraseña.
   - Opcional: `.\scripts\publicar-iis.ps1 -UsarMiCuenta` ejecuta los sitios con tu usuario de Windows y usa la misma base que Visual Studio; pide tu usuario (`whoami`) y tu **contraseña** (no el PIN).
4. Crea los sitios en los puertos 8080 y 8081.
5. Configura las variables de entorno de producción: `ASPNETCORE_ENVIRONMENT=Production` y una **clave JWT aleatoria** (`Jwt__Clave`) que no queda en el repositorio.
6. Comprueba `http://localhost:8081/health`.

## 3. Publicar manualmente (alternativa con Visual Studio)

1. Clic derecho en **SedsImports.Api → Publicar → IIS-Carpeta → Publicar** (genera `C:\inetpub\SedsImports\Api`). Repetir con **SedsImports.Web**.
2. En el **Administrador de IIS**:
   - *Grupos de aplicaciones → Agregar*: `SedsImports.Api`, versión de .NET CLR **Sin código administrado**. Configuración avanzada → **Identidad** → *Cuenta personalizada* (tu usuario) y **Cargar perfil de usuario = True**. Repetir para `SedsImports.Web`.
   - *Sitios → Agregar sitio web*: nombre `SedsImports.Api`, grupo `SedsImports.Api`, ruta `C:\inetpub\SedsImports\Api`, puerto **8081**. Repetir para la web con el puerto **8080**.
   - Sitio de la API → **Editor de configuración** → `system.webServer/aspNetCore` → `environmentVariables` → agregar `ASPNETCORE_ENVIRONMENT = Production` y `Jwt__Clave = (una clave de 32 caracteres o más)`.
3. Iniciar los dos sitios.

## 4. Validar el despliegue (evidencias)

| Prueba | Resultado esperado |
|---|---|
| http://localhost:8081/health | `{"estado":"Healthy", ...}` con la base de datos y la identidad en *Healthy* |
| http://localhost:8081/swagger | Documentación de la API; *Authorize* con el token de `POST /api/auth/login` |
| `GET http://localhost:8081/api/vehiculos` sin token | **401** |
| http://localhost:8080 | Pantalla de inicio de sesión; entrar con `ernesto.rivas@sedimports.com` / `Seds2026` |
| Panel de inicio, botón **Actualizar** | Los indicadores se actualizan sin recargar la página |
| Vehículos → escribir en el buscador | La tabla se filtra mientras se escribe |
| `C:\inetpub\SedsImports\Api\logs` | Archivo `api-AAAAMMDD.log` con cada petición |

## 5. Problemas comunes

| Síntoma | Solución |
|---|---|
| **HTTP Error 500.19** | Falta el Hosting Bundle o no se reinició IIS (`iisreset`). |
| **HTTP Error 500.30 / 500.31** | Revisar `logs\` del sitio. Suele ser la cadena de conexión o la clave JWT (debe tener 32 caracteres o más). |
| **HTTP Error 503** | El grupo de aplicaciones no pudo iniciar: con `-UsarMiCuenta`, el usuario o la contraseña no son correctos. Ejecuta el script sin `-UsarMiCuenta` (cuenta del sistema). |
| La API no abre la base de datos | El grupo de aplicaciones debe tener **Cargar perfil de usuario = True** (LocalDB es por cuenta). Alternativa: SQL Server Express con la cadena `Server=.\SQLEXPRESS;...`. |
| La web dice "No se pudo conectar con el servicio (API)" | Iniciar el sitio `SedsImports.Api` y revisar `Api:UrlBase` en `appsettings.Production.json`. |
| No se guardan fotografías | Dar permiso de *Modificar* a la cuenta del grupo de aplicaciones sobre `Web\wwwroot\uploads`. |

## 6. Otra opción: Azure App Service

1. Crear una **Azure SQL Database** y dos **App Service** (Windows o Linux, .NET 10).
2. Publicar desde Visual Studio (*Publicar → Azure → Azure App Service*).
3. En cada App Service → *Configuración*: cadena de conexión `SedsImportsDB`, `ASPNETCORE_ENVIRONMENT=Production`, `Jwt__Clave` (API) y `Api__UrlBase` (web), y `Cors__OrigenesPermitidos__0` con la dirección de la web.
