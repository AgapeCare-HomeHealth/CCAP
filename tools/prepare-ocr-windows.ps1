# Downloads the pinned Windows Poppler release at build time. No admin installation.
# Run from the CCAP repository root: pwsh ./tools/prepare-ocr-windows.ps1
param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = '26.09.0-0'
$expectedHash = '7a6f256a0ddf7536182246a5733331bf4677cbcc34f4663774947ad34556c8d0'
$url = "https://github.com/oschwartz10612/poppler-windows/releases/download/v$version/Release-$version.zip"
$destination = Join-Path $root 'CCAP.API/Tools/poppler/windows/Library'
$temp = Join-Path ([System.IO.Path]::GetTempPath()) ('ccap-poppler-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $temp | Out-Null
try {
    $archive = Join-Path $temp 'poppler.zip'
    Write-Host "Downloading pinned Poppler v$version from GitHub..."
    Invoke-WebRequest -Uri $url -OutFile $archive
    $actual = (Get-FileHash $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expectedHash) { throw "Poppler SHA-256 mismatch; refusing untrusted binary! Expected $expectedHash, got $actual" }
    $extract = Join-Path $temp 'unpacked'
    Expand-Archive -Path $archive -DestinationPath $extract
    $renderer = Get-ChildItem -Path $extract -Filter 'pdftoppm.exe' -File -Recurse | Where-Object { $_.Directory.Name -eq 'bin' -and $_.Directory.Parent.Name -eq 'Library' } | Select-Object -First 1
    if (-not $renderer) { throw 'Poppler archive does not contain expected Library/bin/pdftoppm.exe' }
    $sourceLibrary = $renderer.Directory.Parent.FullName
    if (Test-Path $destination) { Remove-Item $destination -Recurse -Force }
    New-Item -ItemType Directory -Force $destination | Out-Null
    Copy-Item -Path (Join-Path $sourceLibrary '*') -Destination $destination -Recurse -Force
    $deployedExe = Join-Path $destination 'bin/pdftoppm.exe'
    if (-not (Test-Path $deployedExe)) { throw 'Poppler copy failed.' }
    Write-Host "Windows Poppler installed for CCAP at $deployedExe"
    & $deployedExe -v
} finally {
    if (Test-Path $temp) { Remove-Item $temp -Recurse -Force }
}
