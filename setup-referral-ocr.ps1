# Convenience entry point for Windows local development / Windows Plesk publish.
# English tessdata is bundled in CCAP.API/tessdata, no download required.
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'tools/prepare-ocr-windows.ps1')
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
