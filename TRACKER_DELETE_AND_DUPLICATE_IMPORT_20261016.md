# CCAP Scheduling Tracker: bulk delete and duplicate protection

Changes:
- Reuses Tracker visit checkboxes for Delete Selected with confirmation.
- `POST /api/scheduling/visits/bulk-delete`: permission `scheduling.manage`, max 500 unique visit IDs, scoped to the signed-in scheduling owner. Deletes visits permanently using MediatR and repository.
- Existing CC bulk assignment remains unchanged.
- Schedule import preview identifies duplicates already in the user's schedule and repetitions inside the same uploaded file; shows Skip Duplicate instead of Overwrite.
- Import commit always skips existing matches and repeated rows in the commit request; no overwriting of existing visits.
- Matching currently uses normalized patient name, visit date, time block, and visit type. The clinical edge case of two legitimate identical visits was deferred. This is a conservative skip rule, **not** a definitive patient-identity check.
- Excel/CSV template and existing settings are unchanged. No migration needed for these features.

Important limitations:
- Concurrent overlapping imports are not guaranteed duplicate-free without a database uniqueness/idempotency mechanism, which is intentionally deferred pending the clinical matching decision.
- The bulk deletion is irreversible and has no dedicated persistent audit entry added by this patch. Backup the database and verify permissions before production use.
- Database foreign-key restrictions may prevent deleting visits linked to other records; no broad cascade or related record deletion was introduced.
- No .NET build/runtime test was available here. Build and test on a backed-up database in Visual Studio before deploying.

QA:
1. Import workbook once, then import the same workbook again. Preview should classify existing rows as duplicates; second commit should add zero.
2. Import a workbook containing two equal visit rows; the second should be marked Skip Duplicate.
3. Import a workbook containing 3 new rows and 2 duplicates; commit should save only 3 new rows.
4. Select 2 Tracker visits, cancel deletion; both remain. Confirm deletion; they disappear after refresh; patient/referral records remain.
5. Verify `scheduling.view` without `scheduling.manage` cannot call delete endpoint.
6. Validate CC assignment, editable fields, pagination, Month tab and schedule upload.
