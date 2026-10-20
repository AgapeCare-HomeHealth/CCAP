# CCAP OCR – GitHub/Plesk deployment

## What's included

- `CCAP.API/tessdata/eng.traineddata` – bundled English language model from the installed Debian `tesseract-ocr-eng` package; upstream [tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast), Apache-2.0. SHA-256 `7d4322bd2a7749724879683fc3912cb542f19906c83bcc1a52132556427170b2`. **Obtained from the local system package; not downloaded from GitHub for this archive.**
- `CCAP.Infrastructure/Referrals/ReferralDocumentExtractionService.cs` – resolves the published application's `tessdata` folder and, on Windows, a bundled Poppler executable; host configuration overrides are still supported.
- `tools/prepare-ocr-windows.ps1` – downloads **pinned** official Windows Poppler from [oschwartz10612/poppler-windows releases](https://github.com/oschwartz10612/poppler-windows/releases/tag/v26.09.0-0), validates SHA-256, stages its entire `Library` tree. No admin installation required.
- `.github/workflows/ccap-plesk-windows-publish.yml` – builds Windows API and Web publish artifacts including OCR resources. Removes appsettings files from artifacts to avoid shipping secrets; configure them via hosting settings.
- `Dockerfile.ocr-api` – Linux API image installs Linux Poppler and Tesseract packages; the included language model is copied into the image.

## Why Poppler isn't directly inside source Git

Poppler is native and differs between Windows and Linux. Bundling Windows binaries into every Git commit bloats your repository and cannot run on Linux. This package includes a **verified build-time GitHub download step**, not the 43 MB Windows release binaries. The published Windows artifact includes them after the setup script runs. Linux Docker installs Linux native packages at image build time.

### Windows (local Visual Studio or Windows Plesk)

1. From the repository root, execute: `powershell -ExecutionPolicy Bypass -File .\setup-referral-ocr.ps1` (or `pwsh ./tools/prepare-ocr-windows.ps1`). Requires Internet during preparation.
2. Verify `CCAP.API/Tools/poppler/windows/Library/bin/pdftoppm.exe` exists.
3. `dotnet publish CCAP.API/CCAP.API.csproj -c Release -o ./publish/api`.
4. Verify `publish/api/tessdata/eng.traineddata` and `publish/api/Tools/poppler/windows/Library/bin/pdftoppm.exe` exist.
5. Deploy **the publish output** to Plesk, not raw source. Configure database and JWT values via protected hosting settings; never commit credentials.
6. For automated deployment, use the **CCAP Windows Plesk publish** GitHub Actions workflow and download the generated build artifacts. Native Tesseract dependencies from the .NET package must also be compatible with Windows/IIS runtime.

### Linux Plesk with Docker

- Build with `docker build -f Dockerfile.ocr-api -t ccap-api:ocr .`.
- The image installs Poppler/Tesseract from the Linux package manager, sets `PdfToPpmPath=/usr/bin/pdftoppm`, and uses `/app/tessdata`.
- Docker support and container access must be enabled in your Plesk subscription. Deploy `CCAP.Web` separately.

### Linux Plesk without Docker

- Host must allow installation of `poppler-utils`, `tesseract-ocr`, and native dependencies and provide .NET 10 ASP.NET Core support; otherwise use a separate OCR service.

## Configuration

Shared defaults (`appsettings*.json`):
```json
"ReferralDocumentOcr": { "TessdataPath": "tessdata", "PdfToPpmPath": "pdftoppm", "Dpi": 185, "MaxPages": 25 }
```

Published Windows builds automatically use the bundled Poppler even with default `PdfToPpmPath=pdftoppm`. Linux uses `pdftoppm` from `PATH` or a supplied absolute path. The relative language-data path is resolved from `AppContext.BaseDirectory`, not the machine's working directory. Overrides: `ReferralDocumentOcr__TessdataPath` and `ReferralDocumentOcr__PdfToPpmPath`.

## Critical deployment checks

- You **still need to identify whether Plesk runs Linux or Windows**, .NET 10 availability, and whether native executables/containers are allowed.
- Validate OCR tools in the production hosting environment, not only in Visual Studio.
- Scanned multi-page PDFs may need asynchronous background processing to avoid request timeouts.
- Do not upload real patient files into GitHub or CI test logs. Protect document handling and audit access to PHI.
- **Licenses:** tessdata_fast Apache-2.0 (included); Poppler and bundled DLLs may have GPL and other licenses, review corresponding licenses and redistribution duties before shipping binary artifacts to clients.
- No database migration or changes to scheduling were included. Existing EF Core migration issues remain out of scope.

## Repository security cleanup in this package

- Removed real SQL Server credentials from `CCAP.API/appsettings.Production.json` (production `DefaultConnection` is now empty). Set `ConnectionStrings__DefaultConnection` in Plesk/server environment instead.
- Removed commented-out SQL Server passwords in `CCAP.API/appsettings.Development.json`.
- Removed a commented-out obsolete Web API URL and normalized both Development appsettings files to valid strict JSON.
- Original uploaded archives may still contain leaked credentials. **Rotate them and audit Git history**; editing this archive does not revoke previously exposed secrets.
- Plesk Windows deployments also need `Jwt__Key`, appropriate ASP.NET Core hosting support, and HTTPS set via protected hosting configuration. Never rely on the placeholder JWT key in the repository.
