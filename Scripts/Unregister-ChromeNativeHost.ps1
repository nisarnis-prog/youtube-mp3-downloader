<#
.SYNOPSIS
    Removes this application's Chrome native messaging host registry key for the
    current user.

.DESCRIPTION
    Deletes only the HKCU key 'com.nisar.youtube_mp3_downloader' under
    'Software\Google\Chrome\NativeMessagingHosts'. No other registry data is
    touched.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$hostName = "com.nisar.youtube_mp3_downloader"
$registryKey = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\$hostName"

if (Test-Path -LiteralPath $registryKey) {
    Remove-Item -LiteralPath $registryKey -Recurse -Force
    Write-Host "Removed registry key: $registryKey" -ForegroundColor Green
}
else {
    Write-Host "No registry key to remove: $registryKey" -ForegroundColor Yellow
}