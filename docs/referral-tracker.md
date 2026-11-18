# Referral Tracker Enhancements

## Approved behavior

| Capability | Care Coordinator | Administrator | Other roles on the tracker |
| --- | --- | --- | --- |
| View list and preview saved entries | Yes | Yes | No |
| Filter by assigned Care Coordinator | Yes | Yes | No |
| Print / Save as PDF | Yes | Yes | No |
| Add a tracker entry | Yes, existing permission | Yes | No |
| Edit fields on an assigned entry | Existing tracker fields | Yes | No |
| Edit all business fields on any active entry | No | Yes | No |
| Archive an active entry | No | Yes | No |

The Administrator edit form covers referral date, first and last name, MRN, referral type, posted-on value, estimated discharge date, status, source, insurance, assigned Care Coordinator, all three call notes, and resolution. Referral number, primary key, and system timestamps remain system-managed.

For a simpler workflow, select a patient's underlined name to open the saved-entry preview. The patient name uses the same font size as other values in the list. The table keeps status as referral information. Each user who can edit an entry sees a prominent **Edit** button; Administrators get the full form, and Care Coordinators get the fields allowed by the existing assigned-entry rules. Administrators select one or more rows with checkboxes and choose **Archive selected**. The selected names are shown in a confirmation before the batch is archived. Selection is limited to the current page and clears when filters or pages change.

Archive is a soft delete. Archived entries disappear from active list and export queries. The database retains the entry and stores the archive timestamp and Administrator user ID. No restore screen is included in this change.

## Print / PDF

The print action exports the full filtered result set, not only the current page. It prints the fields shown in the tracker table, omits MRN, includes a generation timestamp and applied filters, and opens the browser print dialog where the user can print or choose **Save as PDF**. The API query uses the same search, status, source, insurance, and assigned-coordinator filters as the active list.

## CQRS and clean-architecture flow

- The API controller validates the authenticated role and maps HTTP contracts to application requests.
- `GetReferralListQuery` and `GetReferralListForExportQuery` read through `IReferralTrackerRepository`.
- `UpdateReferralTrackerDetailsCommand` preserves assigned Care Coordinator editing. Its handler checks the actor and assignment before saving.
- `UpdateReferralTrackerEntryCommand` validates and applies the Administrator's full business-field edit.
- `ArchiveReferralTrackerEntryCommand` records the actor and archive time through the domain entity and repository.
- `ArchiveReferralTrackerEntriesCommand` validates and archives a selected batch in one unit-of-work save; every selected active record is resolved before any entity is changed.
- `ReferralTrackerRepository` applies the shared filters and excludes archived entries from active reads.

## API changes

- `GET /api/referrals` — paged active list, with `coordinatorId` now applied as the assigned Care Coordinator filter.
- `GET /api/referrals/export` — all active entries matching the same filters, restricted to Care Coordinator and Administrator roles.
- `GET /api/referrals/assignment-users` — active Care Coordinator choices for the filter and edit form.
- `GET /api/referrals/assigned-coordinators` — assigned Care Coordinator choices used by the list filter.
- `PUT /api/referrals/{id}/tracker` — existing assigned-entry tracker edits.
- `PUT /api/referrals/{id}/tracker/full` — Administrator-only full business-field update.
- `DELETE /api/referrals/{id}/tracker` — Administrator-only soft archive.
- `POST /api/referrals/tracker/archive` — Administrator-only soft archive for 1–100 selected referral IDs.

The tracker list, assignment lookup, and export require both the `referrals.view` permission and the Care Coordinator or Administrator role. Tracker creation requires the matching role and `referrals.manage` permission. Full edits and archive require Administrator role and `referrals.manage`.

## Bug fixes and additions

- Fixed the assigned Care Coordinator filter contract: the API parameter now passes through the CQRS query and repository to filter results.
- Fixed role leakage from the broad `referrals.view` permission on the tracker list and export endpoints; seeded Clinician accounts no longer gain tracker-list access from that permission alone.
- Added a saved-entry read-only preview distinct from the existing unsaved bulk-entry preview.
- Replaced inline table editing with a high-visibility **Edit** button and edit forms for both roles, each enforcing its existing allowed fields.
- Made the patient name a clear preview link and added Administrator row selection with an explicit, confirmed **Archive selected** action.
- Added Administrator-only editing for all editable business fields, including patient name, MRN, referral date, assignment, and first marketing call.
- Added Administrator-only archive with a confirmation prompt and retained actor/time metadata.
- Added full-result print/PDF output that respects active list filters.

## Database deployment

Apply migration `20261117120000_AddReferralTrackerArchive` before using archive operations. It adds nullable `ArchivedAt` and `ArchivedByUserId` columns to `dbo.ReferralTrackerEntries`; existing rows remain active.
