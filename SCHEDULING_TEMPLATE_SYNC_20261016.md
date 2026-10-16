# CCAP Scheduling import/template sync — 2026-10-16

- Replaced `CCAP.Web/wwwroot/templates/CC_Sched_Import_Template.xlsx` with the provided updated workbook; the existing Download Template URL continues to work.
- Import mapping uses column headings, not positions. The supplied sheet headers are: Date, Day (Auto), Time Block, Patient Name, Confirmation Status, Call Notes, Assigned Clinician, Type of Visit, Visit Status, Notes / Flag.
- Visit ID is intentionally not required or imported. Existing server-side ID generation and duplicate detection remain unchanged.
- Both .xlsx and .csv remain supported; the CSV importer now supports quoted multiline fields (e.g., Call Notes containing newlines), and more reliably recognizes BOM, non-breaking spaces, and zero-width characters in headers.
- Hidden Validation_Data is maintained in the supplied .xlsx. CSV cannot preserve dropdown validation; this is expected.
- No changes to API configuration, database schema, migration files, CQRS handler boundaries, or Scheduling Calendar UI.

## QA to perform in Visual Studio
1. Run Download Template and verify the updated 10-column workbook and dropdowns.
2. Populate one valid visit and preview `.xlsx` upload.
3. Save Template sheet as UTF-8 `.csv` and preview upload, including notes with comma, quote, and multiline text.
4. Verify patient name, clinician and status matching in the preview; ensure import warnings are resolved.
5. Commit and refresh calendar/tracker to confirm ID generation and data persist.
6. Verify existing records and edit flows remain unchanged.

No .NET build or API/DB integration test was performed in this environment.
