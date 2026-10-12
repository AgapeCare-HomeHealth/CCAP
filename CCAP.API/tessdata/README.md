# Referral OCR language data

Scanned referral PDFs use Tesseract OCR. Place the English trained-data file here:

`eng.traineddata`

Recommended source:
https://github.com/tesseract-ocr/tessdata_fast/blob/main/eng.traineddata

The API looks in `./tessdata` by default. You can override the location with:

`ReferralDocumentOcr:TessdataPath`

For Windows hosting, the TesseractOCR NuGet package supplies the native OCR binaries. The Microsoft Visual C++ 2022 runtime required by those binaries must also be installed on the server.
