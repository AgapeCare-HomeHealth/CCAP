# Admin Calendar Import

## Approved scope

Calendar Import is available to Administrator/Admin roles only. It accepts `.xlsx` workbooks with one or more schedule tabs and `.csv` schedule files. Each workbook tab containing the `Patient Name` header is read; tabs without schedule data are ignored. The supplied CC schedule template is available from the page.

Before preview, the administrator selects one active Care Coordinator for the entire uploaded file. Every schedule inserted from that batch is assigned to the selected CC, including schedules for unmatched patients. This assignment overrides a matched patient's primary CC for that batch. The selected CC is saved with the batch and shown in preview, progress, history, row logs, and error downloads.

The import shows a row-by-row preview before it can be queued. It records each batch and row so an administrator can follow progress, open past row results, or download a CSV of failed and duplicate rows.

## Validation and duplicate rules

- Patient name, date, time block, confirmation status, and visit type are required to import a row. Invalid dates, missing time blocks, and invalid time ranges stay visible in the preview as failed rows; they do not abort otherwise valid tabs or rows.
- A patient/date/time-block/visit-type match is an exact duplicate. It is blocked during preview and checked again immediately before saving, against schedules across all users.
- Exact repeated rows in the same upload are also blocked.
- A patient with a different visit type/discipline on the same date receives a review warning. The administrator can proceed after reviewing the warning.
- Two or three distinct visits per week are allowed. There is no weekly frequency limit in this approved scope.
- Cancelled schedules are excluded from duplicate matching, so a new schedule can be entered after cancellation, including a next-day or later reschedule.
- A CCAP patient match is optional. When matched, the visit links to that patient; the batch-selected CC is assigned whether or not a patient match is found. Unmatched patient and clinician names are retained on the schedule.
- Unmatched or blank clinician details and unmatched patients are warnings. They do not prevent import.

## CQRS and clean structure

The Admin page calls the Web API, and the API sends preview, confirm, history, row-log, and error-download requests through MediatR commands/queries. Import orchestration is behind `ICalendarImportApplicationService`; schedule parsing and matching remain behind `IScheduleImportService`. The API background worker calls `ICalendarImportProcessor` to process queued rows. Domain batch/row entities own import state transitions, and EF Core persistence is configured in Infrastructure.

The persistence changes are split into two migrations. `20261117144500_AddCalendarImport` creates `CalendarImportBatches` and `CalendarImportRows`, with explicit schema and column names on the row-to-batch foreign key. `20261117161000_AddCalendarImportCareCoordinatorAssignment` adds the selected Care Coordinator reference to each batch. That field is nullable to preserve batches created before this feature; new previews always require an active Care Coordinator, and an old unassigned preview cannot be queued. Existing queued batches retain the prior patient-coordinator assignment behavior while they finish processing. The API applies pending EF migrations on startup when the existing migration configuration enables it. For manual deployment, apply pending migrations in timestamp order using the project’s standard migration procedure.

## Admin page and API

- Page: `/admin/calendar-import`
- `GET /api/calendar-import/care-coordinators` — active Care Coordinators available for assignment.
- `POST /api/calendar-import/preview` — upload an Excel workbook or CSV and save a reviewable preview.
- `POST /api/calendar-import/{batchId}/confirm` — queue ready rows for background import.
- `GET /api/calendar-import/{batchId}` — batch counts and current status.
- `GET /api/calendar-import/{batchId}/logs` — per-row outcomes.
- `GET /api/calendar-import` — recent batch history.
- `GET /api/calendar-import/{batchId}/errors` — CSV of failed and duplicate rows, including sheet, row, status, and reason.

Every route requires the Administrator or Admin role. The API enforces this requirement independently of the Admin navigation and page.

## Status and recovery

Rows are `Ready`, `Processing`, `Inserted`, `Duplicate`, or `Failed`. Batches are `Preview`, `Queued`, `Processing`, `Completed`, `Partial`, or `Failed`. Queueing is permitted only once and only when at least one row is ready. The worker rechecks exact duplicates before insertion, records row-level outcomes, and can resume ready/processing rows after an interrupted run.

## Related documents

- [Calendar schedule access](calendar-schedule-access.md)
- [Referral Tracker enhancements](referral-tracker.md)
