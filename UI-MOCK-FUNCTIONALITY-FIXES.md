# CCAP UI Mock Functionality Fixes

## Current mode

The Web project remains switchable with:

```json
"MockData": {
  "Enabled": true
}
```

When enabled, the affected UI actions operate against the in-memory `MockDataStore` and do not require CCAP.API.

## Fixed areas

### Roles
- Add New Role modal implemented.
- Edit Role modal implemented.
- Delete Role confirmation modal implemented.
- Manage Permissions modal remains functional.
- Role search/status filters now actually filter the displayed list.
- Mock create/update/delete operations update the in-memory store.
- API create/update/delete contracts were added under `api/admin/roles` for later API mode.

### Dashboard
- Task filter chips now filter the task list.
- View All Tasks resets the filter.
- Dashboard task `Open` buttons now navigate to `/tracker/patient/{PatientId}`.
- Mock dashboard tasks now contain stable patient IDs.

### Patient Profile
- Edit Patient modal implemented in the patient header.
- Mock edits persist in the `MockDataStore` and are reflected after reload.
- Clinical `Add Service` modal implemented with mock ordered-service state.
- Communication `Add` button opens a working mock communication modal.
- Documents `Upload`, `View`, `Download`, and `Replace` buttons now perform mock actions.
- Insurance `Add Authorization`, `New Activity`, and `Upload` buttons now open working mock forms.
- Timeline `Filter` button now applies the selected filter criteria and displays the applied criteria.
- Existing Overview quick actions continue to switch patient tabs.

## API transition

The Web services retain the API path when `MockData:Enabled` is false. Role CRUD API endpoints are now present in `CCAP.API` and use the existing `roles.manage` authorization policy.

Patient edit is currently implemented for mock mode; the API-side patient update command should be completed before using patient editing with `MockData:Enabled=false`.
