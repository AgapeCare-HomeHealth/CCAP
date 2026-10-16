# CCAP Tracker and Import Preview layout refinement

- Import warnings are summarized in one Notice above the preview table instead of repeated in individual rows.
- If duplicates appear in the preview, users must confirm that duplicates will be skipped before proceeding.
- Existing server-side duplicate skipping/rechecking is unchanged; the confirmation is UI-level.
- Tracker Delete Selected is moved to the far side of the toolbar and disabled until at least one row is checked.
- All other files, including original appsettings and workbook, are unchanged from the prior full solution ZIP.
- No database migration required. Run a full Visual Studio build and functional QA.
