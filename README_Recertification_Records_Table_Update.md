# CCAP Recertification Records Table Update

- Replaced the single Recertification Details date/status inputs with a repeatable table.
- Each row stores a recertification received date and PCP & PT notification status (YES, NO, or N/A).
- Rows are persisted in `Patients.RecertificationRecordsJson` and returned with workflow details.
- Removed the duplicate PCP & PT Notified item from the Recert or Discharge checklist and its progression gate/task creation.
- The existing discharge compliance requirements remain unchanged.
- Added EF Core migration `20261112190000_AddRecertificationRecords`.

Note: This update supports multiple recertification records per current patient record. It does not implement Patient Episodes or certification-period workflow instances.


## Recertification status summary update

- Added live Complete, Outstanding, and Pending totals above the recertification history table.
- Complete counts records with a received date and PCP & PT status of YES or N/A.
- Outstanding counts records whose PCP & PT status is NO.
- Pending counts actual records that are neither complete nor outstanding (including a missing received date or unanswered status).
- The dropdown keeps the explanatory labels: `Yes — Complete`, `No — Outstanding`, and `N/A — Complete`.
- Counts are calculated from saved/in-memory recertification rows and update as rows are edited or added/removed. No row is generated and no Pending count is added when no further recertification occurred.
