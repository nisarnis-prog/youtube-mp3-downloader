<#
.SYNOPSIS
    Registers the Chrome native messaging host 'com.nisar.youtube_mp3_downloader'
    for the current user (HKCU) for local development testing.

.DESCRIPTION
    Determines the absolute path of the built YouTubeMp3NativeHost.exe,
    generates an effective native-messaging manifest next to it (with absolute
    paths resolved), and writes the HKCU registry value Chrome looks for.

    Pass -ExtensionId (from chrome://extensions) so Chrome accepts native
    messages from the loaded unpacked extension. Without it, the manifest is
    written with a placeholder origin and a warning is shown.

.PARAMETER ExtensionId
    The 32-character extension ID shown in chrome://extensions for the loaded
    unpacked 'Nisar MP3 Downloader Companion' extension.

.PARAMETER Config
    Build configuration the native host was built with (Debug or Release).
    Defaults to Debug.

.EXAMPLE
    .\Register-ChromeNativeHost.ps1 -ExtensionId abcdefghijklmnopabcdefghijklmnop
#>
[CmdletBinding()]
param(
    [string]$ExtensionId,
    [string]$Config = "Debug"
)

$ErrorActionPreference = "Stop"

$hostName = "com.nisar.youtube_mp3_downloader"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

if ($Config -notin @("Debug", "Release")) {
    throw "Config must be 'Debug' or 'Release' (got '$Config')."
}

$hostDir = Join-Path $projectRoot "NativeHost\bin\$Config\net10.0"
$hostExe = Join-Path $hostDir "YouTubeMp3NativeHost.exe"
$manifestPath = Join-Path $hostDir "$hostName.json"

if (-not (Test-Path -LiteralPath $hostExe)) {
    throw "Native host executable not found at '$hostExe'. Build it first:`n  dotnet build '$(Join-Path $projectRoot 'NativeHost\YouTubeMp3NativeHost.csproj')'"
}

if ([string]::IsNullOrWhiteSpace($ExtensionId)) {
    $allowedOrigins = @("chrome-extension://<EXTENSION_ID>/")
    Write-Warning "No -ExtensionId supplied. 'allowed_origins' will contain a placeholder."
    Write-Warning "Open chrome://extensions, copy the ID of 'Nisar MP3 Downloader Companion',"
    Write-Warning "then run this script again with -ExtensionId <ID> (or edit '$manifestPath')."
}
else {
    $allowedOrigins = @("chrome-extension://$ExtensionId/")
}

$manifest = @{
    name             = $hostName
    description      = "Native messaging host for Nisar MP3 Downloader Companion"
    path             = $hostExe
    type             = "stdio"
    allowed_origins  = $allowedOrigins
} | ConvertTo-Json

Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding utf8

$registryRoot = "HKCU:\Software\Google\Chrome\NativeMessagingHosts"
$registryKey = Join-Path $registryRoot $hostName

if (-not (Test-Path -LiteralPath $registryRoot)) {
    New-Item -Path $registryRoot -Force | Out-Null
}

New-Item -Path $registryKey -Force | Out-Null
Set-ItemProperty -Path $registryKey -Name "(Default)" -Value $manifestPath

Write-Host ""
Write-Host "Native messaging host registered for the current user." -ForegroundColor Green
Write-Host "  Registry key : $registryKey"
Write-Host "  Manifest     : $manifestPath"
Write-Host "  Host exe     : $hostExe"
Write-Host "  Allowed      : $($allowedOrigins -join ', ')"
Write-Host ""
Write-Host "Restart Chrome, then retry the extension." -ForegroundColor Yellow