# Referral List implementation notes

## Added
- Referral List navigation entry alongside New Referral and Draft Referrals.
- CQRS read path: `GetReferralListQuery` -> `IReferralRepository.GetPagedListAsync` -> API `GET /api/referrals`.
- Paginated referral list UI with patient/MRN search and status/source/insurance filters.
- Multi-referral entry panel starts with five referral forms; Add Row appends an entry and × removes an entry before submission.
- Each valid entry is submitted through the existing referral intake service/API path, preserving the existing intake command handler and its validation/duplicate checks.

## Important behavior
- Bulk submission uses the existing single-referral CQRS intake endpoint sequentially. It is not an all-or-nothing database transaction: if a later entry fails, earlier entries remain saved and the UI reports the number already saved.
- MRNs duplicated within the current batch are rejected before submission. The existing API also rejects an MRN already present in the patient store.
- The supplied domain model does not currently contain referral call-attempt 1/2/3 or resolution fields, nor a referral status summary aggregate. Those screenshot-only fields/cards were not fabricated or backed by placeholder patient data.
- The UI's compact multi-entry form intentionally captures the minimum fields required by the existing intake validation (first name, last name, date of birth), plus MRN, source, and insurance. The full existing New Referral workflow remains available for detailed intake and document handling.

## Architecture
- Read behavior is implemented as a MediatR query/handler in Application.
- Persistence is exposed through `IReferralRepository` and implemented in Infrastructure.
- API exposes the query through the existing referrals controller and permission policy.
- Blazor calls the API through the existing `ReferralIntakeService`; no direct DbContext or Infrastructure dependency was added to Web.
- Existing Clean Architecture boundaries and current New Referral / Draft Referral workflows are retained.

## Verification
- Source files and solution structure were inspected after editing.
- A .NET build could not be run in this environment because the .NET SDK/MSBuild is not installed. Build and runtime verification should be performed in Visual Studio 2026 or a machine with the .NET 10 SDK before deployment.


## Referral Tracker UI and live Care Coordinator assignments (2026-11-03)

- The Referral List now uses a refined filter panel, clearer table heading, sticky table headers, and a larger card-based multi-referral entry dialog.
- Add Referrals loads coordinator choices from `GET api/referrals/assignment-users`; it does not use static coordinator names.
- The API returns only active accounts with a non-empty email/password hash and an active role containing `Coordinator`. Password hashes are never returned to the client.
- Bulk referral creation submits `CoordinatorId` (the real `ApplicationUser.UserId`). The API validates the selected account again and stores the authoritative full name for tracker display.
- The existing referral intake wizard uses the same assignment-user endpoint to keep both intake paths consistent.
