# CCAP Notification Center

## Purpose

The topbar bell is the global notification center. Notifications are **derived from workflow data and dates**; users do not manually create workflow alerts.

The same notification feed is reused by:

- Topbar bell — cross-patient actionable alerts
- Patient Overview — active alerts for the selected patient
- Orders & Care — underlying records and management actions

## Flow

```text
Patient / Order / Task / Visit
            |
            v
     Notification Service
            |
      +-----+------+
      |            |
   Mock Mode     API Mode
      |            |
 MockDataStore  GET /api/notifications
      |            |
      +-----+------+
            |
            v
    UserNotificationDto
       /           \
      v             v
 Topbar Bell   Patient Overview
```

## Mock mode

`NotificationCenterService` calls `MockDataStore.BuildGlobalNotifications()`.

The mock calculation currently generates alerts from:

- POC/OASIS `OrderDate` and signature status
  - `< 30 days`: no signature alert
  - `30-59 days`: Warning / follow-up
  - `60+ days`: Critical / escalation
- Lab order `DueDate`
  - within 3 days: reminder
  - past due: critical
- Wound supply `NeededBy`
  - within 3 days: reminder
  - past due: critical
- Foley `NextDueDate`
  - within 3 days: reminder
  - past due: critical
- Unverified referring fax information

The notification IDs are deterministic from the underlying record ID and notification type, so refreshing the bell does not create duplicate logical alerts.

## User experience

The bell displays only notifications for users who have the `notifications.view` permission.

Clicking a notification marks it read for the current UI session and navigates to:

```text
/tracker/patient/{PatientId}
```

Patient Overview displays the same feed filtered to its `PatientId`.

## Orders & Care changes

The following are intentionally **not manual notification-entry screens**:

- Patient Notifications
- Orders Management Alerts

Orders & Care now focuses on the underlying records:

- Fax information
- POC/OASIS order records and signature status
- Lab orders
- Wound supplies
- Foley catheter changes

The 30/60-day POC/OASIS alert state is calculated from the order date and signature status.

## API contract

The Web UI calls:

```http
GET /api/notifications
Authorization: Bearer <JWT>
```

Response shape:

```json
[
  {
    "notificationId": "00000000-0000-0000-0000-000000000000",
    "patientId": "00000000-0000-0000-0000-000000000000",
    "patientName": "John Smith",
    "type": "Task",
    "title": "Complete assessment",
    "message": "Complete the assessment before the due date.",
    "severity": "Warning",
    "dueDate": "2026-08-15T10:00:00Z",
    "isRead": false,
    "createdAt": "2026-08-12T10:00:00Z"
  }
]
```

The API endpoint requires `notifications.view`.

The current API implementation derives notifications from persisted `PatientTask` and `Visit` records because those are the current persisted workflow sources in the project. POC/OASIS, lab, supply, Foley, and fax notification rules can be added to the same API service once their corresponding persistent records are exposed by the API/domain layer.

## Prerendering safety

The topbar loads notifications from `OnAfterRenderAsync` rather than `OnInitializedAsync`. This is intentional because API mode uses `CcapApiClient`, which retrieves the JWT from protected browser storage. Protected browser storage must not be accessed during static prerendering.

## Permissions

Current notification permission:

```text
notifications.view
```

The existing management permission remains available for future persistent/manual notification workflows:

```text
notifications.manage
```

Fax viewing is also separated from fax management:

```text
fax.view
fax.manage
```

## Switching Mock -> API

In development:

```json
"MockData": {
  "Enabled": true
}
```

Use `false` when the API implementation is ready:

```json
"MockData": {
  "Enabled": false
}
```

The Razor components do not change when switching modes. Only `NotificationCenterService` changes its data source.
