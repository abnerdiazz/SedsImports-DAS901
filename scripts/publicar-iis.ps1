<#
.SYNOPSIS
    Publica la API y la aplicación web de SED's Imports en IIS (Windows).

.DESCRIPTION
    1. Compila y publica los dos proyectos en modo Release (C:\inetpub\SedsImports\Api y \Web).
    2. Crea los grupos de aplicaciones (sin código administrado, como pide ASP.NET Core).
    3. Crea los sitios: API en http://localhost:8081 y Web en http://localhost:8080.
    4. Configura las variables de entorno de producción (entorno y clave JWT) en cada sitio.

    Requisitos: IIS con la consola de administración, el "ASP.NET Core Hosting Bundle" de .NET 10
    y SQL Server LocalDB. Ejecuta PowerShell COMO ADMINISTRADOR desde la carpeta raíz del repositorio:

        powershell -ExecutionPolicy Bypass -File .\scripts\publicar-iis.ps1

.NOTES
    Por defecto los sitios usan la cuenta del sistema (LocalSystem) y LocalDB crea para ella su propia
    base SedsImportsDB con los datos de ejemplo. Para usar tu cuenta de Windows (y la misma base que
    Visual Studio) agrega -UsarMiCuenta: el script pedirá tu usuario y contraseña (no el PIN).
#>
param(
    [string]$Destino = "C:\inetpub\SedsImports",
    [int]$PuertoWeb = 8080,
    [int]$PuertoApi = 8081,
    # Por defecto los sitios se ejecutan con la cuenta del sistema (LocalSystem), que funciona con
    # cuentas Microsoft y no pide contraseña. Con -UsarMiCuenta se ejecutan con tu usuario de Windows
    # y usan la misma base de LocalDB que Visual Studio.
    [switch]$UsarMiCuenta
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration

function Paso($texto) { Write-Host "`n==> $texto" -ForegroundColor Cyan }

# Clave JWT aleatoria de 64 caracteres para producción (no se guarda en el repositorio).
$clave = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object { [char]$_ })

$credencial = $null
if ($UsarMiCuenta) {
    Paso "Credenciales para los grupos de aplicaciones (tu usuario de Windows)"
    $credencial = Get-Credential -Message "Usuario de Windows con el que se ejecutarán los sitios (ej. $env:COMPUTERNAME\$env:USERNAME)"
} else {
    Paso "Los sitios se ejecutarán con la cuenta del sistema (LocalSystem): LocalDB crea su propia base con los datos de ejemplo"
}

Paso "Deteniendo los sitios si ya existen"
foreach ($sitio in "SedsImports.Web", "SedsImports.Api") {
    if (Test-Path "IIS:\Sites\$sitio") { Stop-Website -Name $sitio }
}

Paso "Publicando la API y la aplicación web (Release)"
dotnet publish .\SedsImports.Api\SedsImports.Api.csproj -c Release -o "$Destino\Api"
dotnet publish .\SedsImports.Web\SedsImports.Web.csproj -c Release -o "$Destino\Web"

# Carpetas que la aplicación necesita escribir: logs y fotografías subidas.
New-Item -ItemType Directory -Force -Path "$Destino\Api\logs", "$Destino\Web\logs", "$Destino\Web\wwwroot\uploads" | Out-Null

foreach ($app in @(
    @{ Nombre = "SedsImports.Api"; Carpeta = "$Destino\Api"; Puerto = $PuertoApi },
    @{ Nombre = "SedsImports.Web"; Carpeta = "$Destino\Web"; Puerto = $PuertoWeb })) {

    Paso "Configurando $($app.Nombre) en http://localhost:$($app.Puerto)"

    if (-not (Test-Path "IIS:\AppPools\$($app.Nombre)")) { New-WebAppPool -Name $app.Nombre | Out-Null }
    $pool = "IIS:\AppPools\$($app.Nombre)"
    Set-ItemProperty $pool -Name managedRuntimeVersion -Value ""          # Sin código administrado
    Set-ItemProperty $pool -Name processModel.loadUserProfile -Value $true
    Set-ItemProperty $pool -Name processModel.setProfileEnvironment -Value $true
    if ($credencial) {
        Set-ItemProperty $pool -Name processModel -Value @{
            userName = $credencial.UserName; password = $credencial.GetNetworkCredential().Password; identityType = 3
        }
    } else {
        Set-ItemProperty $pool -Name processModel.identityType -Value 0   # LocalSystem
    }

    if (Test-Path "IIS:\Sites\$($app.Nombre)") { Remove-Website -Name $app.Nombre }
    New-Website -Name $app.Nombre -PhysicalPath $app.Carpeta -ApplicationPool $app.Nombre -Port $app.Puerto -Force | Out-Null

    # Permisos de escritura para logs y fotografías.
    $cuenta = if ($credencial) { $credencial.UserName } else { "SYSTEM" }
    icacls $app.Carpeta /grant "$($cuenta):(OI)(CI)M" /T /Q | Out-Null

    # Variables de entorno del sitio (system.webServer/aspNetCore/environmentVariables).
    $ruta = "system.webServer/aspNetCore/environmentVariables"
    Clear-WebConfiguration -PSPath "IIS:\Sites\$($app.Nombre)" -Filter $ruta -ErrorAction SilentlyContinue
    Add-WebConfigurationProperty -PSPath "IIS:\Sites\$($app.Nombre)" -Filter $ruta -Name "." -Value @{ name = "ASPNETCORE_ENVIRONMENT"; value = "Production" }
    if ($app.Nombre -eq "SedsImports.Api") {
        Add-WebConfigurationProperty -PSPath "IIS:\Sites\$($app.Nombre)" -Filter $ruta -Name "." -Value @{ name = "Jwt__Clave"; value = $clave }
    }
}

Paso "Iniciando los sitios"
Start-WebAppPool -Name "SedsImports.Api" -ErrorAction SilentlyContinue
Start-WebAppPool -Name "SedsImports.Web" -ErrorAction SilentlyContinue
Start-Website -Name "SedsImports.Api"
Start-Website -Name "SedsImports.Web"

Paso "Validando el despliegue"
# La primera vez, LocalDB crea la instancia y la base de datos: puede tardar hasta 2 minutos.
$salud = $null
for ($i = 1; $i -le 24 -and -not $salud; $i++) {
    Start-Sleep -Seconds 5
    try { $salud = Invoke-RestMethod "http://localhost:$PuertoApi/health" -TimeoutSec 30 } catch { }
}
if ($salud) { Write-Host "API: $($salud.estado)" -ForegroundColor Green }
else { Write-Host "La API todavía no responde. Revisa $Destino\Api\logs y el Visor de eventos." -ForegroundColor Yellow }

Write-Host "`nListo:" -ForegroundColor Green
Write-Host "  Aplicación web:  http://localhost:$PuertoWeb"
Write-Host "  API (Swagger):   http://localhost:$PuertoApi/swagger"
Write-Host "  Salud de la API: http://localhost:$PuertoApi/health"
