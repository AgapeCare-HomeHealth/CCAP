# Notification Center Implementation Changes

## Completed in this build

- Added a central `NotificationCenterService` in `CCAP.Web`.
- Added `UserNotificationDto` as the UI/API notification contract.
- Topbar bell now loads real notification data instead of a hard-coded empty state.
- Notification count is calculated from unread actionable notifications.
- Notification dropdown displays severity, patient, message, and due date.
- Clicking a notification navigates to `/tracker/patient/{PatientId}`.
- Mock mode calculates alerts from existing patient-care records.
- Removed manual Patient Notification creation from Orders & Care.
- Removed manual Order Alert creation from Orders & Care.
- Patient Notes remain in the patient Overview and are no longer presented as an Orders & Care management card.
- Patient Overview displays the same active alert feed filtered to the current patient.
- Added `GET /api/notifications` to CCAP.API.
- API notification endpoint requires `notifications.view`.
- Added `fax.view` permission alongside `fax.manage`.
- Topbar notification loading is deferred until `OnAfterRenderAsync` to avoid ProtectedLocalStorage/JS interop during prerendering.

## Current API scope

The API endpoint currently derives notifications from persisted `PatientTask` and `Visit` records because those are the existing persisted workflow sources in the current domain model.

The Web contract is intentionally independent from the persistence model. Once persistent POC/OASIS, lab, wound-supply, Foley, and fax records are exposed through the API, their date-based rules can feed the same endpoint without changing the topbar UI.

## Mock notification rules

- POC/OASIS unsigned for 30-59 days -> Warning
- POC/OASIS unsigned for 60+ days -> Critical
- Lab due within 3 days -> Warning/Info; overdue -> Critical
- Wound supply needed within 3 days -> Warning; overdue -> Critical
- Foley change due within 3 days -> Warning; overdue -> Critical
- Unverified referral fax -> Warning

No workflow notification is created manually by the user.
