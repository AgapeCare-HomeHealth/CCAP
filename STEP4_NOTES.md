# CCAP Scheduling - Step 4

Implemented backend CQRS for manually added schedules.

## Included
- AddSchedulesCommand + AddScheduleItem
- AddSchedulesCommandValidator
- AddSchedulesCommandHandler
- AddSchedulesResult
- POST /api/scheduling endpoint
- Visit domain factory for manual schedules
- Visit fields: TimeBlock, ConfirmationStatus, CallNotes, NotesFlag
- Visit repository duplicate check using patient/date/time block
- Clinician validation (active + Clinician role)
- Lookup validation for ConfirmationStatus, VisitType, VisitStatus
- EF Core migration: 20260925000000_AddSchedulingFieldsToVisits
- Calendar query DTO exposes the new scheduling fields

## Not included yet
The Blazor Save Schedules button is intentionally not connected to this endpoint. That is the next UI/API integration step.

## Database
Apply the new EF Core migration before testing persistence:
`20260925000000_AddSchedulingFieldsToVisits`

The environment used to prepare this step does not have the .NET SDK installed, so a local Visual Studio build/migration application is still required.
