# CCAP Referral Auto-Population Regression Fix — 2026-10-27

## Scope

This patch restores the referral PDF extraction safeguards in the latest CCAP solution while retaining the updated calendar files. No database schema or migration changes are included.

## Behaviors restored

- **MRN:** Only explicitly labelled, structurally validated MRN candidates are considered. One candidate can populate an empty MRN field. Multiple distinct candidates are shown with source page/section and require Care Coordinator confirmation before moving past Upload.
- **Primary Care Physician:** Only a plausible name associated with an explicit PCP/primary-care label is accepted. Signature blocks, numbers, facilities, and mixed-field text are rejected.
- **Primary Insurance:** OCR candidate is cleaned in Infrastructure, then selected only if it exactly matches an active Insurance dropdown option (case/outer whitespace ignored). Lookup failures do not create a new selection.
- **Ordered Services:** PDF extraction does not check or replace service selections. The Care Coordinator selects services manually.
- **Mixed PDFs:** OCR runs when identity/safety-critical fields are missing or a page has little/no text-layer content. Text-layer and OCR page evidence are combined before MRN conflict detection.
- **Portable OCR deployment:** The API project explicitly copies the English trained data and bundled Windows Poppler files into build and publish output. Linux hosts should use the Linux/Docker Poppler setup, not the Windows binaries.

## Main files

- `CCAP.Infrastructure/Referrals/ReferralDocumentExtractionService.cs`
- `CCAP.Infrastructure/Referrals/ReferralExtractionSafety.cs`
- `CCAP.Application/Features/Referrals/DTOs/ReferralDocumentExtractionResult.cs`
- `CCAP.Web/Features/Tracker/ReferralIntake/Models/ReferralDocumentExtractionDto.cs`
- `CCAP.Web/Features/Tracker/ReferralIntake/Models/ReferralIntakeModel.cs`
- `CCAP.Web/Features/Tracker/ReferralIntake/Components/UploadStep.razor`
- `CCAP.Web/Features/Tracker/ReferralIntake/Components/ReferralInformationStep.razor`
- `CCAP.Web/Features/Tracker/ReferralIntake/Pages/ReferralIntake.razor`
- `CCAP.API/CCAP.API.csproj`

## Manual QA checklist

1. Build the full solution in Visual Studio with .NET 10 SDK.
2. Upload a referral with a single clearly labelled MRN: confirm the candidate may populate the MRN field.
3. Upload a packet with different MRNs from different facilities: confirm MRN stays blank and the review panel lists each distinct candidate with page/section.
4. Try Next before confirming: the wizard must remain on Upload.
5. Select a candidate and confirm: Next should proceed and Referral Information should show that MRN.
6. Choose the manual MRN option: confirm the MRN remains blank for manual entry in Referral Information.
7. Upload a document with signature text near the PCP field: PCP must not be populated from the signature.
8. Upload insurance text that is not an active dropdown option (for example, OCR noise such as `73)`): insurance must remain blank.
9. Upload an exact active dropdown insurance name with different casing/outer spaces: it should match the active option.
10. Confirm Ordered Services remain unchanged/unchecked after upload.
11. Re-test calendar, tracker search, CC assignment, delete, import preview, and scheduling to verify no unrelated behavior changed.

## Verification note

The archive was inspected and the source changes were applied. A .NET build and end-to-end browser/API test still need to be run in a Windows development environment with the .NET 10 SDK; the current editing environment does not have `dotnet` installed.
