# Calendar feature merge notes

Base: `CCAP_workflow_progress_badges` (workflow progress/compliance badges preserved).
Calendar source: `CCAP_Full_Solution_Tracker_CC_Migrations_Repaired_20261014`.

Merged calendar additions include the updated month/tracker UI, calendar styling and DTOs, schedule tracker detail editing, bulk Care Coordinator assignment, API endpoints, scheduling option data, Visit entity fields, persistence mapping, repository methods, and the `AddVisitCareCoordinator` migration.

The base project's workflow/compliance components, `ComplianceStatus` mapping, migration history, referral-intake files, and unrelated project configuration were retained.

Before deploying, build the solution and apply the new EF Core migration to the target database. The migration adds nullable `Visits.CareCoordinatorId` with an FK to `ApplicationUsers.UserId`; it does not delete existing visit data.
