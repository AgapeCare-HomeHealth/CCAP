# CCAP — Scanned Referral PDF Auto-Population (2026-10-20)

## Scope

This patch uses the latest submitted October 20 solution, preserving the existing
Blazor wizard, Web/API contracts, Clean Architecture and CQRS boundaries.

- `CCAP.Infrastructure/Referrals/ReferralDocumentExtractionService.cs`: digital
  text, full-page PDF rendering via Poppler, OCR via Tesseract, cross-page parsing,
  bounded resolution and safe temporary-file cleanup.
- `CCAP.Infrastructure/Referrals/ScannedReferralDemographics.cs`: demographic
  reconciliation using *patient-labeled* evidence, including names, birth dates,
  gender and address. Conflicting phone numbers and uncertain emergency contacts
  remain blank for manual verification.
- `CCAP.Web/.../UploadStep.razor`: do not overwrite text the user has already entered;
  preserve existing wizard-to-form mapping.
- `CCAP.Web/Features/Authentication/Services/CcapApiClient.cs` and `CCAP.Web/Program.cs`:
  use a longer 5-minute timeout only for PDF extraction, not other API requests.
- `Dockerfile.ocr-api` and `.dockerignore`: provider-independent OCR container
  with Poppler/Tesseract and no baked-in appsettings secrets.

## Cloud/container usage

Build from the repository root:

```sh
docker build -f Dockerfile.ocr-api -t ccap-api-ocr .
```

Pass database, JWT and other service configuration using environment variables
or your cloud secret manager. This repo copy still contains original local
`appsettings` files for the developer environment; do NOT commit or distribute
real passwords. The Docker build context excludes appsettings JSON.

For non-container deployments, install `pdftoppm` and the Tesseract English
language data on the **API host**, not on the browser/client. Configure:

- `ReferralDocumentOcr:PdfToPpmPath` (e.g. `/usr/bin/pdftoppm` or Windows .exe)
- `ReferralDocumentOcr:TessdataPath` (directory containing `eng.traineddata`)
- `ReferralDocumentOcr:Dpi` (default 185)
- `ReferralDocumentOcr:MaxPages` (default 25, maximum 100)

Docker runtime uses a Linux OCR executable. Paths are environment-specific;
there are no hardcoded developer machine drive paths in source code.

## Real referral test

The uploaded 21-page *image-only* referral was used as a manual OCR fixture.
Do **not** add the real patient PDF or any extracted PHI to the repository.

Sampled OCR pages included 1, 5, 8 and 13. Those pages provided recognizable
patient-labelled names, birth date, gender and address. A patient-labelled
telephone appeared, but contradictory contact context means it must be reviewed.
This is a selected-pages OCR extraction check, **not** a passed .NET integration test.

### Acceptance criteria

1. Rebuild all five projects on a machine with .NET 10 SDK.
2. Run the API with working Poppler and English Tesseract data.
3. Upload the 21-page PDF through Referral Intake → Upload PDF.
4. Confirm Step 2 patient name, DOB, gender and address against the source PDF.
5. Do not accept wrong providers/pharmacy numbers as the patient's phone.
6. Missing/unreliable emergency-contact values should remain blank.
7. Repeat with structured, two-column, narrative and scanned synthetic PDFs.
8. Confirm replacing a PDF does not overwrite unrelated manually entered data.
9. Verify errors are user-friendly when OCR binaries are missing.
10. Confirm production access logging does not include PDF text or patient values.

### Limitations

- The upload currently runs synchronously. For production scale and higher page
  counts, use a durable background job worker and an upload-status UI.
- Full .NET build, native-Tesseract runtime integration and browser testing have
  not been performed in the packaging environment.
- OCR confidence alone is **not** proof of clinical accuracy. Staff must review
  extracted fields; contradictory or missing fields require manual confirmation.
- Generic extraction may still need additional training/validation for unfamiliar
  facilities. Do not use patient-name similarity as a unique identity key.
