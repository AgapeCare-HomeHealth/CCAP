$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$tessdata = Join-Path $root 'CCAP\CCAP.API\tessdata'
New-Item -ItemType Directory -Force -Path $tessdata | Out-Null

$url = 'https://github.com/tesseract-ocr/tessdata_fast/raw/main/eng.traineddata'
$destination = Join-Path $tessdata 'eng.traineddata'

Write-Host 'Downloading Tesseract English OCR data...'
Invoke-WebRequest -Uri $url -OutFile $destination
Write-Host "OCR data installed at $destination"
