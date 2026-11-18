# Calendar Schedule Access

## Approved behavior

| Role | Calendar access |
| --- | --- |
| Care Coordinator | View schedules assigned to or created by them, edit and reschedule within their existing scope, and delete schedules assigned to or created by them. |
| Administrator | View, edit, reschedule, update, and delete schedules across the calendar. |
| Other scheduling roles | Keep their existing permission and ownership scope. |

Care Coordinators can use the delete action on a schedule in their own calendar. Administrators can delete any calendar schedule, including those assigned to another Care Coordinator. Delete requires confirmation and removes the schedule entry. The API repeats the role checks so access is not controlled by the UI alone.

## CQRS and clean-architecture flow

- `SchedulingController` checks the signed-in role and sends the delete command with the Administrator and Care Coordinator role flags.
- `DeleteScheduleVisitsCommandHandler` validates the requested IDs, loads the full permitted set, rejects partial or inaccessible requests, then removes all selected visits in one unit-of-work save.
- `IVisitRepository.GetForDeletionAsync` scopes Care Coordinators to schedules they created or that are assigned to them; Administrators resolve any selected schedule IDs.
- Schedule detail updates, drag-to-reschedule, and status updates preserve Care Coordinator scope and allow Administrators to manage schedules across the calendar.

## API behavior

- `POST /api/scheduling/visits/bulk-delete` — Care Coordinator can delete only their created or assigned schedules; Administrator can delete any selected schedule.
- `PUT /api/scheduling/visits/{id}/details` — Administrators can update any schedule; Care Coordinators keep their existing assignment scope.
- `PUT /api/scheduling/visits/{id}/reschedule` — Administrators can reschedule any schedule; Care Coordinators keep their own assignment scope.

Calendar reads require the `scheduling.view` permission. Standard schedule-management endpoints require `scheduling.manage`; schedule detail updates and deletion also explicitly accept Administrator and Care Coordinator roles, with Care Coordinator access enforced by per-schedule assignment rules.

## Referral Tracker typography fix

The patient name preview link now inherits the same font size as other values in the referral list. Its bold weight, underline, and focus state remain to make the preview action clear.
