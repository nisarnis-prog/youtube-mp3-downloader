# YouTube MP3 Downloader v1.0.0 - pinned developer dependencies
# Run from any directory: powershell -ExecutionPolicy Bypass -File .\Scripts\Setup-Dependencies.ps1
[CmdletBinding()]
param([switch]$ReplaceExisting)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$tools = Join-Path $root 'Tools'
New-Item -ItemType Directory -Path $tools -Force | Out-Null

$expected = @{
    'yt-dlp.exe'  = '66674953FE251B89F4D08C5F0E35E0728679BD67AB3D7D05C0562AF101DD3E7A'
    'ffmpeg.exe'  = '13ED571B647E5D4D2064B73CB413CC1412EE305D715E726A4C9AF75A78E7113F'
    'ffprobe.exe' = '6DD8BA9EFA64AA90C439DEB780E90B779CFE18A4434951BF8B94D2A7BA96B048'
}
$ffmpegUrl = 'https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2026-10-08-13-05/ffmpeg-N-127252-ga25ba44c0c-win64-lgpl.zip'
$ytDlpUrl = 'https://github.com/yt-dlp/yt-dlp/releases/download/2026.08.19/yt-dlp.exe'

function Test-Hash([string]$Path, [string]$Sha256) {
    (Test-Path -LiteralPath $Path -PathType Leaf) -and
        ((Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -eq $Sha256)
}
function Assert-Hash([string]$Path, [string]$Sha256) {
    if (-not (Test-Hash $Path $Sha256)) {
        throw "SHA256 verification failed for $Path. No unverified file will be installed."
    }
}
function Download-File([string]$Url, [string]$Destination) {
    Write-Host "Downloading $Url"
    Invoke-WebRequest -Uri $Url -OutFile $Destination -UseBasicParsing -MaximumRedirection 10
}

# Refuse to overwrite nonmatching local binaries unless explicitly requested.
foreach ($name in $expected.Keys) {
    $dest = Join-Path $tools $name
    if ((Test-Path -LiteralPath $dest) -and -not (Test-Hash $dest $expected[$name]) -and -not $ReplaceExisting) {
        throw "Existing $dest differs from tested v1.0.0. Back it up and rerun with -ReplaceExisting to replace it."
    }
}

$missing = @($expected.Keys | Where-Object { -not (Test-Hash (Join-Path $tools $_) $expected[$_]) })
if ($missing.Count -eq 0) {
    Write-Host 'All three tools already match tested v1.0.0 SHA256 hashes. Nothing downloaded.'
    exit 0
}

$tempDir = Join-Path ([IO.Path]::GetTempPath()) ('youtube-mp3-deps-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
try {
    if ($missing -contains 'yt-dlp.exe') {
        $downloadedYtDlp = Join-Path $tempDir 'yt-dlp.exe'
        Download-File $ytDlpUrl $downloadedYtDlp
        Assert-Hash $downloadedYtDlp $expected['yt-dlp.exe']
    }
    if (($missing -contains 'ffmpeg.exe') -or ($missing -contains 'ffprobe.exe')) {
        $archivePath = Join-Path $tempDir 'ffmpeg.zip'
        Download-File $ffmpegUrl $archivePath
        # The version-specific ZIP may differ from the latest-named ZIP at the archive level.
        # Verify each extracted executable against its pinned SHA256 before installation.
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
        try {
            foreach ($name in @('ffmpeg.exe', 'ffprobe.exe')) {
                if ($missing -notcontains $name) { continue }
                $entries = @($archive.Entries | Where-Object { $_.FullName -match ('(^|/)bin/' + [regex]::Escape($name) + '$') })
                if ($entries.Count -ne 1) { throw "Expected one bin/$name in FFmpeg archive; found $($entries.Count)." }
                $staged = Join-Path $tempDir $name
                $inputStream = $entries[0].Open()
                try {
                    $outputStream = [IO.File]::Create($staged)
                    try { $inputStream.CopyTo($outputStream) } finally { $outputStream.Dispose() }
                } finally { $inputStream.Dispose() }
                Assert-Hash $staged $expected[$name]
            }
        } finally { $archive.Dispose() }
    }
    # Install only after every downloaded file passes verification.
    foreach ($name in $missing) {
        Copy-Item -LiteralPath (Join-Path $tempDir $name) -Destination (Join-Path $tools $name) -Force
        Assert-Hash (Join-Path $tools $name) $expected[$name]
        Write-Host "Verified and installed: $name"
    }
    Write-Host 'Dependency setup complete. Run: dotnet build .\YouTube-MP3-Downloader.csproj -c Release'
} finally {
    Remove-Item -LiteralPath $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
