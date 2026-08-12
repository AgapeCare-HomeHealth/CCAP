# CCAP Mock Care + Role Permissions Build

## Mock mode

Set in `CCAP.Web/appsettings.Development.json`:

```json
{
  "MockData": {
    "Enabled": true
  }
}
```

Set `false` when the Web UI should use the existing API-backed services. The new care-management service currently exposes API-shaped method boundaries and intentionally uses MockData until dedicated API endpoints are implemented.

## Included patient-profile features

The Patient Workflow page now includes an **Orders & Care** tab tied to the selected `PatientId`.

### Fax Information
- Referring fax number
- Referring provider
- Organization
- Document type
- Received date
- Verification flag
- Notes

### Patient Notifications
- Add, mark read, delete
- Patient-specific
- Expiration is capped at 60 days from creation in both UI validation and MockDataStore enforcement

### Patient Notes
- Subject
- Content
- Priority
- Created by/date
- Mark resolved/open
- Delete

### Lab Orders
- Test/lab
- Ordering provider
- Ordered date
- Due date
- Status
- Complete action

### Wound Supplies
- Supply
- Quantity
- Frequency
- Needed-by date
- Status
- Notes

### Foley Catheter Changes
- Change date
- Next due date
- Catheter size
- Balloon size
- Changed by
- Notes

### Orders Management Alerts
- POC/OASIS order type
- Order date
- Automatic 30-day threshold
- Automatic 60-day threshold
- PCP signature status
- Monitoring/Due/Critical/Complete alert level
- Mark signed

## Role restrictions

Mock permissions include:

- `fax.manage`
- `notifications.view`
- `notifications.manage`
- `notes.view`
- `notes.manage`
- `labs.view`
- `labs.manage`
- `supplies.view`
- `supplies.manage`
- `foley.view`
- `foley.manage`
- `orders.view`
- `orders.manage`

Management buttons in Orders & Care are hidden when the current mock user's role does not contain the corresponding `*.manage` permission.

The API permission policy catalog and database seeder were also extended with these permission codes so the eventual API implementation can use the same authorization vocabulary.

## Role permission modal

The latest role-management implementation loads the complete permission catalog first and then the selected role's assigned permission IDs. The modal displays grouped checkbox fields and persists changes in MockData mode.

## Persistence in Mock mode

`MockDataStore` is registered as a singleton so role/permission and patient-care changes remain available across Blazor circuits during the running application instance.

## API transition

The Razor UI uses `PatientCareManagementService` rather than directly depending on `MockDataStore`. Its method boundaries are API-shaped. When the corresponding API endpoints are implemented, the service can switch its `MockData:Enabled=false` path to HTTP calls without changing the Patient Workflow UI.

The API is still the authoritative security boundary for deployed mode; UI permission checks are for presentation/usability and do not replace server-side authorization.
