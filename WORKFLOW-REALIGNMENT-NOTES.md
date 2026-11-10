# CCAP update — Patient Workflow UI preserved

## Patient Workflow
- Restored the existing `ComplianceTab.razor` from the supplied full CCAP source so the Patient Workflow keeps its established compliance-card UI, two-column layout, status dropdowns, badges, counters, spacing, and responsive behavior.
- No replacement workflow board or custom CSS layout is used.
- The existing workflow grouping, compliance components, status controls, and page interactions are preserved.

## Scheduling changes retained
- Add/Edit Schedule use one `CC Notes` field instead of separate Notes/Flag and Call Notes inputs.
- Existing Call Notes are merged into CC Notes when editing an existing visit; new saves write to the combined notes field.
- Updated visit-type fallback/seed values: `DCA` replaces `DC`, standalone `SN` removed, and confirmation label uses `Non-visit DC`.

## Architecture
- Existing Clean Architecture project boundaries and CQRS patterns are retained.
- No new patient-status enum value or speculative Pending -> Active transition was added. The supplied PatientStatus enum does not contain Pending.

## Validation
- ZIP integrity checked.
- A .NET build was not run in this environment. Please build with the solution's target .NET SDK in Visual Studio before deployment.
