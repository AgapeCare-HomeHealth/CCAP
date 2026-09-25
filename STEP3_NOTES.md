# Insurance / Compliance Step 3

## What this step adds
- Real Pre-Auth insurance document upload through the API.
- CQRS command/validator/handler for upload.
- CQRS queries for stored document metadata and stored file access.
- PatientComplianceDocument persistence table.
- Local file storage in Development.
- Server-side file validation and 10 MB request limit.
- Audit log entry for each successful upload.
- Stored document View button in the Compliance UI.

## Data model
Documents are linked to the patient and the compliance requirement code `PRE_AUTH_RECEIVED`.
Multiple uploads are allowed; the UI displays the latest document.

## Storage
Development is configured to use `Local` storage under the API's `App_Data/Files` directory.
Production remains `MetadataOnly` until a real storage provider (for example Azure Blob) is configured.
Local storage sanitizes the supplied file name and prefixes stored files with a GUID.

## Database
Migration: `20260925000001_AddPatientComplianceDocuments`

Before testing, build the solution and confirm the migration appears as pending in Package Manager Console. Apply it only to the intended development database with `Update-Database`.

## Important
This step does NOT mark `PRE_AUTH_RECEIVED` complete automatically. Uploading the document and marking the compliance requirement complete remain separate actions, matching the requested workflow.
