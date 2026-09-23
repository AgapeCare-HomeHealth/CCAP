# CCAP — Care Coordinator Action Platform

## 1. Project Overview

**CCAP (Care Coordinator Action Platform)** is a care-coordination workflow application for managing patient referrals, patient-care episodes, compliance requirements, assignments, tasks, communications, documents, clinical workflow information, discharge, journey closure, and archived patient history.

The project is implemented as a .NET 10 solution using **Clean Architecture**, **CQRS/MediatR**, the **Repository Pattern**, **ASP.NET Core Web API**, **Blazor Web**, **EF Core**, and **SQL Server**.

### Primary goals

- Replace spreadsheet-driven patient workflow tracking with a centralized application.
- Preserve the workflow concepts used by the current Excel tracker.
- Track referrals from intake through the end of a patient-care episode.
- Separate patient identity from the lifecycle of an individual care episode.
- Provide permission-based access for administrators, care coordinators, and schedulers.
- Keep an auditable history of patient actions and workflow changes.
- Support discharge, journey closure, archiving, and future new referrals for previously discharged patients.

---

# 2. Solution Structure

```text
CCAP/
├── CCAP.sln
├── CCAP.slnx
├── ARCHITECTURE.md
├── BUILD_AND_DATABASE.md
│
├── CCAP.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── CCAP.Domain.csproj
│
├── CCAP.Application/
│   ├── Abstractions/
│   ├── Common/
│   ├── Features/
│   └── CCAP.Application.csproj
│
├── CCAP.Infrastructure/
│   ├── Identity/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Seed/
│   ├── Migrations/
│   └── CCAP.Infrastructure.csproj
│
├── CCAP.API/
│   ├── Authorization/
│   ├── Contracts/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings*.json
│
└── CCAP.Web/
    ├── Features/
    ├── Layout/
    ├── Services/
    └── CCAP.Web.csproj
```

---

# 3. Architecture

CCAP follows Clean Architecture.

## Domain

`CCAP.Domain`

Contains:

- Entities
- Enums
- Domain-level business rules

The Domain layer should not depend on:

- Entity Framework Core
- ASP.NET Core
- Blazor
- Controllers
- Infrastructure implementations

Important entities include:

- `Patient`
- `Referral`
- `ReferralDraft`
- `ReferralDocument`
- `ComplianceRecord`
- `Visit`
- `PatientTask`
- `PatientServiceOrder`
- `PatientCareLog`
- `PatientAuditLog`
- `CallNote`
- `Activity`
- `ApplicationUser`
- `LookupOption`
- `ServiceType`
- `Location`
- `Discipline`
- `Role`
- `Permission`

## Application

`CCAP.Application`

Contains:

- CQRS commands
- CQRS queries
- MediatR handlers
- DTOs
- Validators
- Repository abstractions
- Identity abstractions
- File-storage abstractions
- Application use cases

The Application layer does not directly depend on EF Core.

## Infrastructure

`CCAP.Infrastructure`

Contains:

- EF Core
- SQL Server persistence
- `AppDbContext`
- EF migrations
- Repository implementations
- JWT implementation
- Password hashing
- Database seeding
- Azure Blob file-storage implementation

`AppDbContextFactory` is used for EF Core design-time operations.

## API

`CCAP.API`

Contains:

- HTTP controllers
- Authentication configuration
- JWT bearer authentication
- Authorization policies
- API contracts
- Swagger/OpenAPI
- Database migration startup behavior

## Web

`CCAP.Web`

Contains the Blazor presentation layer:

- Dashboard
- Referrals
- Referral drafts
- Patient lists
- Patient workflow
- Active patients
- Archived patients
- Administration
- Tasks
- Notifications
- Patient workflow tabs/components

---

# 4. Technology Stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor Web |
| Architecture | Clean Architecture |
| Application Pattern | CQRS |
| Mediator | MediatR 14.2.0 |
| ORM | Entity Framework Core 10.0.11 |
| Database | SQL Server |
| Authentication | JWT Bearer |
| Authorization | ASP.NET Core policies + permission claims |
| API Documentation | Swagger / Swashbuckle |
| File Storage Abstraction | `IFileStorage` |
| Cloud Storage | Azure Blob Storage |
| UI styling | Bootstrap / project CSS |
| Database migrations | EF Core migrations |

---

# 5. Authentication

The normal authentication flow is:

```text
CCAP.Web
   ↓
POST /api/auth/login
   ↓
LoginCommand
   ↓
Infrastructure user/password verification
   ↓
JWT generated
   ↓
JWT stored by the web client
   ↓
Bearer token used for protected API requests
```

The Blazor application uses an authentication-state provider to construct the current user from JWT claims.

JWT settings are configured under:

```json
"Jwt": {
  "Key": "...",
  "Issuer": "CCAP.API",
  "Audience": "CCAP.Web",
  "ExpireMinutes": 480
}
```

## Production security

Never deploy the development JWT key.

Replace:

```text
CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_BEFORE_DEPLOYMENT
```

with a long random secret stored securely.

Do not commit production secrets to source control.

---

# 6. Authorization and Roles

CCAP uses permission-based authorization.

The intended roles are:

- **Administrator**
- **Care Coordinator**
- **Scheduler**

Permissions are mapped from JWT claims to ASP.NET Core authorization policies.

Examples of permission areas include:

- Patient access
- Referral access
- Workflow actions
- Compliance actions
- Lookup management
- Role management
- Service type management
- Tasks
- Notifications
- Administration

Infrastructure should not reference ASP.NET Core authorization APIs. Authorization policy configuration belongs in `CCAP.API`.

---

# 7. Patient vs. Patient-Care Episode

A critical business rule is that a **patient is not the same thing as a care episode**.

A patient may receive care more than once.

Conceptually:

```text
Patient
│
├── Episode 1
│     ├── Referral
│     ├── Insurance
│     ├── SOC
│     ├── Admission
│     ├── Ongoing Care
│     └── Discharge / Closed
│
└── Episode 2
      ├── New Referral
      ├── Insurance
      ├── SOC
      ├── Admission
      ├── Ongoing Care
      └── Discharge / Closed
```

Therefore:

- Closing/archiving a previous journey must not mean deleting the patient.
- A discharged patient must remain searchable.
- A later referral should be able to start another care episode.
- Previous episode history must remain available.
- New episode information must not overwrite historical episode information.

> **Implementation note:** the current solution still stores many workflow fields directly on `Patient`. If full multi-episode behavior is required, the long-term domain model should introduce a dedicated `PatientEpisode`/`CareEpisode` aggregate and move episode-specific fields and relationships to that aggregate.

---

# 8. Current Patient Journey

The workflow is based on the current Excel tracker and the clarified business requirements.

The intended journey is:

```text
Referral
   ↓
Pre-Intake
   ↓
Insurance
   ↓
SOC
   ↓
Admission
   ↓
Ongoing Care
   ↓
Final Outcome
   ↓
Discharge
   ↓
Patient Journey Closed
   ↓
Archive
```

`Re-Cert` and `ROC (Resumption of Care)` are referral types rather than a separate standalone Recertification workflow stage.

---

# 9. Referral

The referral/intake area contains patient and referral information.

## Patient information

- First Name
- Last Name
- Middle Name where applicable
- Date of Birth
- Gender
- Contact information
- Address
- Emergency contact information
- Diagnosis information
- Insurance information

## Referral Type

The intended values are:

- **New** — new patient
- **Repeat**
- **Re-Cert**
- **ROC** — Resumption of Care

## Referral Channel

The intended values are:

- Allscripts
- Careport
- Direct-Fax
- Direct-email
- Direct-phone
- Direct-in-person

## Referral dates

- Initial Referral Date
- Verified Referral Date

## Target SOC

Target SOC is based on the **48-hour requirement**.

The application should calculate/display the target based on the appropriate referral date/time rather than relying on a manually entered target.

## Referral status

The clarified business requirement is:

- Pending
- Discharge

`Case Status` should not be used as a separate referral workflow status.

---

# 10. Pre-Intake

Pre-Intake is the preparation stage before SOC.

It includes the referral information and the information needed to determine whether the referral is ready to progress.

Insurance information is associated with the referral/patient and is also verified through the dedicated Insurance workflow.

---

# 11. Insurance Workflow

**Insurance remains a dedicated workflow/tab.**

Insurance verification should have an explicit completion action.

## Insurance Verification

The user can review/update:

- Primary insurance
- Insurance member ID
- Authorization information
- Approved visits where applicable
- Authorization required
- Verification information
- Verification date
- Verifying user

The user completes verification using:

> **Mark as Completed / Verify Insurance**

The completion should record:

```text
InsuranceVerifiedAt
InsuranceVerifiedByUserId
```

The action is auditable.

Insurance verification is a workflow gate before progressing to SOC where required by the workflow.

---

# 12. Wellness Check

Wellness Check belongs to the referral/pre-intake portion of the journey.

It should not be confused with the clinical SOC visit.

The exact business rules for additional Wellness Check statuses are not defined in the source requirements and should not be invented without confirmation.

---

# 13. SOC

SOC means Start of Care.

The SOC workflow includes:

- Check Date
- Establish Clinician
- SOC target
- Insurance verification as a prerequisite where applicable

The target requirement is:

> **SOC within 48 hours**

The actual SOC information should be recorded separately from the calculated target.

CCAP does not need to become the clinical visit scheduling system.

---

# 14. Admission

Admission follows SOC.

The workflow records:

> **Actual Completion**

Admission completion should be based on the application's admission requirements rather than simply assuming that a SOC date means admission is complete.

---

# 15. Ongoing Care

Ongoing Care is the active period of the patient-care episode.

It is **not** a simple checkbox that becomes complete when every Compliance item is green.

It represents the period during which the patient is actively receiving care and follow-up activities are being managed.

## OASIS

Requirement:

> **OASIS within 48 hours**

## Plan of Care

Requirement:

> **Plan of Care created within 24 hours after OASIS**

The application should track these as workflow milestones.

## Important rule

Do not use:

```text
All Compliance items complete
    ↓
Ongoing Care completed
```

as the terminal workflow rule.

Ongoing Care continues until the episode reaches its final outcome.

---

# 16. 60-Day Episode

The clarified requirement is:

> **Episode Start of Care = 60-day episode**

The application should calculate the episode timeline from the episode/SOC start date.

At the end of the 60-day episode:

> **Automatic discharge may occur.**

The patient may subsequently receive a new referral.

This means the patient remains a historical patient while the completed care journey is closed.

---

# 17. Recertification and ROC

The previous standalone Recertification workflow stage is removed.

Instead:

### Re-Cert

`Re-Cert` is a referral type.

### ROC

`ROC` means:

> Resumption of Care

and is also a referral type.

### Transfer

Transfer/TIF/ROC information should be treated as episode events/context, not automatically as a terminal patient state.

A temporary inpatient transfer is not equivalent to permanent transfer or final discharge.

---

# 18. Compliance

Compliance is where workflow requirements are explicitly confirmed.

Examples include:

- Insurance verification
- DME / Medical Supply
- NOA
- OASIS / SOC
- QA approval
- Orders signed
- Documents uploaded
- Case Mix identified
- Case Mix orders signed
- Case Mix plotted
- SOC feedback where applicable
- Discharge-related requirements

Each requirement can be explicitly marked complete.

The system should record completion information and audit history.

## SOC compliance

`SOC_COMPLIANT` is manually verified by the user.

CCAP should not create a requirement that blocks SOC compliance because a CCAP visit is incomplete when the actual SOC scheduling/visit documentation is managed elsewhere.

---

# 19. Clinical Visits and External EMR

Clinical visit scheduling/documentation is not intended to be duplicated inside CCAP.

The source workflow identifies the external clinical/EMR system as the source for actual visit activity.

CCAP's role is to:

- Track workflow requirements
- Record confirmations
- Track follow-ups
- Record relevant dates/events
- Maintain coordination history

It should not create unnecessary duplicate clinical scheduling actions.

---

# 20. Final Outcome

The final outcome is the actual closing decision for the care episode.

The final outcome UI should provide:

- Outcome selection
- Discharge information
- Transfer information when applicable
- Discharge date
- Discharge feedback
- Discharge requirements
- Complete Discharge / Complete Transfer action

The normal outcome is:

> **Discharge**

---

# 21. Discharge

Discharge is the terminal clinical outcome of the episode.

The Excel workflow contains:

- Discharge Date
- Discharge Feedback From Patient
- Discharge Summary Signed
- NOMNC Signed

The discharge workflow also includes:

> **Discharge OASIS within 5 days**

Discharge requirements must be validated before completing the discharge action.

---

# 22. Patient Journey Closed

Discharged is not the same as the application's final administrative closure.

The intended closing sequence is:

```text
Final Outcome
   ↓
Discharged
   ↓
Discharge Completed
   ↓
Patient Journey Closed
   ↓
Archive
```

The closed journey should retain:

- Final status
- Final outcome
- Discharge information
- Compliance history
- Timeline/history
- Audit history
- Relevant episode information

The user should receive a clear confirmation that the journey is closed.

---

# 23. Archive

Archive means:

> The completed patient-care journey is no longer part of the active workflow.

Archive must **not** mean:

- Delete patient
- Delete history
- Prevent future care
- Overwrite previous episode data

A discharged patient can later receive another referral.

Conceptually:

```text
Closed Episode
     ↓
Archived Episode
     ↓
Existing Patient remains searchable
     ↓
New Referral
     ↓
New Care Episode
```

The project contains an `ArchivedPatients` page for viewing archived patients.

---

# 24. Patient Workflow Tabs

The patient workflow contains components for areas such as:

- Overview
- Insurance
- Compliance
- Clinical
- Patient Care Management
- Documents
- Communication
- Timeline
- Final Outcome / Discharge
- Audit Log

Relevant component locations:

```text
CCAP.Web/Features/Tracker/PatientWorkflow/
├── Pages/
│   └── PatientWorkflow.razor
│
└── Components/
    ├── OverviewTab.razor
    ├── InsuranceTab.razor
    ├── ComplianceTab.razor
    ├── ClinicalTab.razor
    ├── PatientCareManagementTab.razor
    ├── DocumentsTab.razor
    ├── CommunicationTab.razor
    ├── TimelineTab.razor
    ├── AuditLogTab.razor
    ├── DischargeTab.razor
    ├── WorkflowProgress.razor
    └── WorkflowTabs.razor
```

---

# 25. Tasks and Notifications

CCAP supports patient tasks and notifications.

Tasks can be associated with workflow activities and assigned users.

The application includes CQRS commands for task completion.

Tasks should be used for coordination/follow-up actions rather than duplicating external clinical visit scheduling.

---

# 26. Patient Audit History

The system includes patient audit logging.

Important audit information includes:

- Action
- Patient
- User
- Date/time
- Relevant action details

Audit history is important because completed journeys are retained after discharge/archive.

---

# 27. Lookup Management

The project contains a generic `LookupOption` model.

Fields include:

```text
LookupOptionId
LookupType
Code
DisplayName
SortOrder
IsActive
```

Database indexes support lookup-type/code uniqueness and active/sort-order queries.

The administrator can manage lookup values from:

```text
/admin/lookup-options
```

The implementation supports:

- Case-insensitive duplicate checking
- Canonical lookup groups
- Search
- Group filtering
- Active/inactive values
- Sort order
- Create
- Update
- Delete/deactivate

Examples of lookup groups include:

- Gender
- Referral Source
- Priority
- Patient Status
- Yes/No
- Case Mix Type
- Contact Type
- Communication Method
- Confirmation Status
- Visit Type
- Visit Status
- Final Discharge Status
- Insurance

---

# 28. Service Types

Service types are managed separately from generic lookup values.

The application supports service/order types such as:

- SN
- PT
- OT
- ST
- HHA

Service type administration is available under the administration area.

Referenced service types should not be blindly deleted; deactivation is preferred when historical records depend on them.

---

# 29. Referral Drafts

Referral intake supports incomplete drafts.

The intended process is:

```text
Intake/Referral Staff
       ↓
Upload referral information
       ↓
Save incomplete draft
       ↓
Head Care Coordinator
       ↓
Review / complete / verify
       ↓
Submit referral
       ↓
Assignment
```

Draft-related components are under:

```text
CCAP.Web/Features/Tracker/ReferralDrafts/
```

---

# 30. Referral Intake Wizard

The intake workflow is divided into components including:

```text
PatientInformationStep
ReferralInformationStep
UploadStep
AssignmentStep
ReviewStep
WizardHeader
WizardProgress
```

The intake process should support saving incomplete work before final submission where applicable.

---

# 31. API Endpoints

The API is organized into controllers.

Important patient endpoints include:

```text
GET    /api/patients
GET    /api/patients/{id}
GET    /api/patients/{id}/workflow
GET    /api/patients/{id}/care-management
GET    /api/patients/{id}/audit-log

PUT    /api/patients/{id}
PUT    /api/patients/{id}/insurance
PUT    /api/patients/{id}/workflow-details

POST   /api/patients/{id}/insurance/verify
POST   /api/patients/{id}/soc/schedule
POST   /api/patients/{id}/soc/complete
POST   /api/patients/{id}/complete-care
POST   /api/patients/{id}/archive
POST   /api/patients/{id}/compliance/{requirementCode}
POST   /api/patients/{id}/call-notes
POST   /api/patients/{id}/care-logs
POST   /api/patients/{id}/service-orders
POST   /api/patients/tasks/{taskId}/complete
```

Referral endpoints include:

```text
POST /api/referrals/intake
POST /api/referrals/draft
GET  /api/referrals/drafts
GET  /api/referrals/drafts/{draftId}
```

Authentication:

```text
POST /api/auth/login
```

Administration includes endpoints for:

- Roles
- Permissions
- Lookup options
- Service types
- Disciplines

Swagger is enabled in development.

---

# 32. EF Core and Database

The database is SQL Server.

The application uses EF Core migrations as the schema source of truth.

Current migration files include:

```text
CCAP.Infrastructure/Migrations/
├── 20260910235634_InitialCCAPSchema.cs
├── 20260910235634_InitialCCAPSchema.Designer.cs
├── 20260914190632_AddLookupOptionsAndWorkflowFields.cs
├── 20260914190632_AddLookupOptionsAndWorkflowFields.Designer.cs
└── AppDbContextModelSnapshot.cs
```

## Important migration rule

Do not manually edit migration history unless the database schema and migration state have first been verified.

If a table already exists but its migration is not recorded in `__EFMigrationsHistory`, reconcile the schema carefully rather than blindly creating the table again.

---

# 33. Database Configuration

The API reads:

```text
ConnectionStrings:DefaultConnection
```

Example local SQL Server connection:

```text
Server=localhost;Database=CCAP;Trusted_Connection=True;TrustServerCertificate=True;
```

The active environment configuration must be checked because:

```text
appsettings.json
appsettings.Development.json
appsettings.Production.json
```

can override one another.

Always verify the actual database name before running migrations.

---

# 34. Applying Migrations

## Visual Studio Package Manager Console

Set:

```text
Default project: CCAP.Infrastructure
Startup project: CCAP.API
```

Then:

```powershell
Update-Database
```

## CLI

From the solution directory:

```bash
dotnet restore
dotnet build CCAP.sln
dotnet ef database update --project CCAP.Infrastructure --startup-project CCAP.API
```

The API currently calls:

```csharp
await context.Database.MigrateAsync();
```

during startup before database seeding.

This means the API can automatically apply committed migrations when it starts.

---

# 35. Database Reset — Development Only

If development data can be discarded and the migration state is corrupted, the cleanest development reset is:

```powershell
Drop-Database
Update-Database
```

Do **not** use this against production data.

If data must be preserved, inspect:

```sql
SELECT MigrationId
FROM __EFMigrationsHistory
ORDER BY MigrationId;

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'LookupOptions';
```

and reconcile the schema/migration history carefully.

---

# 36. Database Seeding

The project contains database seeding for initial configuration.

Seeder responsibilities include:

- Initial users
- Roles
- Permissions
- Lookup values
- Service types
- Other required reference data

The lookup seeder uses case-insensitive handling to avoid duplicate logical values.

---

# 37. File Storage

The application has an `IFileStorage` abstraction.

Current configuration:

```json
"FileStorage": {
  "Provider": "MetadataOnly"
}
```

In MetadataOnly mode, the application can retain referral document metadata such as:

- Filename
- Content type
- File size

The actual PDF bytes are not necessarily persisted by the MetadataOnly provider.

Azure Blob Storage support is prepared.

Example configuration:

```json
"FileStorage": {
  "Provider": "AzureBlob"
},
"AzureBlob": {
  "ConnectionString": "...",
  "ContainerName": "ccap-files",
  "RootFolder": "referrals"
}
```

Production secrets should be supplied through secure configuration/environment variables.

---

# 38. Running the API

From the solution directory:

```bash
dotnet run --project CCAP.API
```

In development, Swagger is available when the development environment is active.

Check the configured launch settings in:

```text
CCAP.API/Properties/launchSettings.json
```

---

# 39. Running the Web Application

From the solution directory:

```bash
dotnet run --project CCAP.Web
```

The Web application communicates with the API using its configured API base URL/settings.

Check the Web project's service/configuration files when changing deployment environments.

---

# 40. Local Development Checklist

Before running:

1. Install the .NET 10 SDK.
2. Make sure SQL Server is running.
3. Verify the configured database name.
4. Verify the connection string.
5. Restore packages.
6. Build the solution.
7. Apply migrations if required.
8. Start the API.
9. Start the Blazor Web application.
10. Log in with a seeded/admin account.
11. Test referral intake.
12. Test insurance verification.
13. Test SOC.
14. Test admission.
15. Test compliance.
16. Test final outcome/discharge.
17. Test journey closure.
18. Test archive.
19. Verify archived history remains accessible.

---

# 41. Troubleshooting

## `Invalid object name 'LookupOptions'`

Usually means the database does not contain the table expected by the current model/migration.

Check:

```sql
SELECT *
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'LookupOptions';
```

Then inspect:

```sql
SELECT MigrationId
FROM __EFMigrationsHistory
ORDER BY MigrationId;
```

Verify the application is connected to the intended database.

---

## `There is already an object named 'LookupOptions'`

This means the table already exists while EF is attempting to execute a migration that creates it.

The likely cause is:

```text
Database schema
    ≠
EF migration history
```

Do not blindly run another migration.

Verify both the table and `__EFMigrationsHistory` first.

---

## `ApplicationUser cannot be tracked because another instance ... is already being tracked`

This normally happens when an entire EF entity graph is loaded/attached and contains duplicate `ApplicationUser` instances.

For focused updates such as archive/status changes:

- Load the entity as a tracked entity.
- Change only the required fields.
- Call `SaveChanges`.
- Avoid calling `Update()` on a large detached graph.

---

## Blazor `InputDate` ValueExpression error

Reusable Blazor input components such as `InputDate<T>` require a valid `ValueExpression` when `Value` and `ValueChanged` are manually supplied.

The reusable Date/Number/Text field components therefore explicitly provide their value expressions.

---

## Blank Final Outcome tab

The Final Outcome tab must map its selected tab value to the discharge/final-outcome component.

If the tab title is:

```text
Final Outcome
```

the rendering logic must handle the same value.

A mismatch such as:

```text
case "Discharge":
```

when the UI sends:

```text
"Final Outcome"
```

results in no component being rendered.

---

# 42. Clean Architecture Rules for Future Development

When adding features:

### Domain

Put:

- Entity state
- Domain invariants
- Domain methods
- Domain enums

in `CCAP.Domain`.

Do not put EF Core code here.

### Application

Put:

- Commands
- Queries
- Handlers
- Validators
- DTOs
- Interfaces

in `CCAP.Application`.

### Infrastructure

Put:

- EF Core
- Repositories
- Identity implementations
- Database operations
- External storage implementations

in `CCAP.Infrastructure`.

### API

Put:

- Controllers
- HTTP request/response contracts
- JWT configuration
- Authorization policies

in `CCAP.API`.

### Web

Put:

- Razor components
- Pages
- UI state
- API client behavior
- Presentation-specific services

in `CCAP.Web`.

---

# 43. Repository Pattern

The Application layer defines repository interfaces.

Infrastructure implements them.

Example:

```text
Application
    IPatientRepository
          ↑
          │ implements
          │
Infrastructure
    PatientRepository
```

Do not directly inject `AppDbContext` into Application handlers.

For update operations, prefer focused tracked queries when the entity is being modified.

Avoid updating a complete entity graph when only one or two fields need to change.

---

# 44. CQRS

Commands represent state changes.

Examples:

```text
CompleteInsuranceVerificationCommand
CompleteComplianceCommand
CompleteSocCommand
CompleteCareCommand
ArchivePatientCommand
CompleteTaskCommand
UpdatePatientCommand
UpdateInsuranceCommand
```

Queries read data.

Examples:

```text
GetPatientsQuery
GetPatientWorkflowQuery
GetPatientCareManagementQuery
GetPatientAuditLogQuery
GetServiceTypesQuery
GetLookupOptionsQuery
```

Commands should validate business rules before modifying state.

---

# 45. Workflow State Principles

Workflow status should be **derived from real data** where possible.

Avoid duplicating the same state in multiple unrelated database fields.

Examples:

- Insurance completion comes from insurance verification.
- Compliance completion comes from compliance records.
- SOC completion comes from the SOC workflow.
- Admission completion comes from admission requirements.
- Final outcome comes from the finalization/discharge action.
- Journey closure comes from the closure/archive process.

The workflow UI should reflect actual persisted state rather than maintaining an independent fake progress tracker.

---

# 46. Excel Workflow Mapping

The current workflow is based on the Excel tracker concepts supplied for the project.

Important workflow tracker fields include:

```text
REFERRAL DATE
PATIENT NAME
ASSIGNED CLINIC
STATUS
NOTES
INSURANCE
PRE-AUTH RECEIVED
PRE-AUTH DUE
NUMBER of VISIT
PRO RECEIVED
AVS RECEIVED
H&P
F2F Received
PCP Confirmed
DME / Med Supply
Signed Consents
NOTICE of ADMISSION (NOA)
OASIS / SOC Compliant
Orders Signed by Physician
Docs Uploaded
SOC Feedback from Patient
Case Mix Identified
Case Mix Type
Case Mix Plotted
QA Approval – PO/POC Ready for Faxing
TRANSFER to INPATIENT FACILITY (TIF)
TIF DATE
RESUMPTION of CARE (ROC)
ROC DATE
RECERTIFICATION
RECERT DATE
DISCHARGE Date
DISCHARGE Feedback From Patient
DISCHARGE Summary Signed
NOMNC Signed
```

The application should preserve these business concepts while using appropriate normalized entities and workflow records.

---

# 47. Important Business Rules

1. **Insurance remains a workflow/tab.**
2. Insurance verification has an explicit **Mark as Completed** action.
3. `Case Status` is removed from the intended referral UI/workflow.
4. Referral Type supports:
   - New
   - Repeat
   - Re-Cert
   - ROC
5. Referral Channel supports:
   - Allscripts
   - Careport
   - Direct-Fax
   - Direct-email
   - Direct-phone
   - Direct-in-person
6. Target SOC uses the 48-hour requirement.
7. SOC includes check date and established clinician.
8. Admission records actual completion.
9. Ongoing Care is an active care period, not a simple compliance checkbox.
10. OASIS is due within 48 hours.
11. Plan of Care is created within 24 hours after OASIS.
12. The episode is based on a 60-day start-of-care period.
13. Automatic discharge may occur after 60 days.
14. A discharged patient may receive a new referral.
15. Re-Cert is a referral type, not a standalone Recertification workflow stage.
16. ROC is Resumption of Care.
17. TIF is a temporary inpatient transfer and is not automatically terminal.
18. Discharge OASIS is due within 5 days.
19. Final Outcome is the terminal outcome decision.
20. Discharge is a final outcome.
21. Patient Journey Closed is the administrative/workflow closure after final outcome.
22. Archive removes the completed journey from the active working area but retains history.
23. Archive must not delete the patient.
24. SOC compliance is manually confirmed in CCAP.
25. CCAP should not create unnecessary duplicate clinical visit scheduling/completion tasks.
26. Audit history must remain available after closure.

---

# 48. Deployment

For deployment to a hosting environment such as Plesk:

### API

Publish:

```bash
dotnet publish CCAP.API -c Release
```

Upload the resulting publish output to the API application directory.

### Web

Publish:

```bash
dotnet publish CCAP.Web -c Release
```

Upload the resulting publish output to the Web application directory.

### Production configuration

Configure:

- SQL Server connection string
- JWT secret
- JWT issuer
- JWT audience
- File storage
- Azure Blob settings if enabled
- Allowed hosts
- Environment-specific API/Web settings

Do not use development secrets in production.

---

# 49. CI/CD Considerations

A production CI/CD pipeline should generally:

```text
Git push
   ↓
Restore
   ↓
Build
   ↓
Test
   ↓
Publish API
   ↓
Publish Web
   ↓
Apply database migration
   ↓
Deploy API
   ↓
Deploy Web
   ↓
Health check
```

Database migrations should be handled deliberately in production. Automatic application of migrations at API startup is convenient for development but should be reviewed against the production deployment strategy.

---

# 50. Recommended Future Improvements

The current project can be strengthened further by:

### 1. Dedicated Patient Episode aggregate

Move episode-specific fields from `Patient` into:

```text
PatientEpisode
```

so one patient can have many independent care journeys.

### 2. Episode-level relationships

Associate:

- Referrals
- Compliance records
- Visits
- Tasks
- Documents
- Care logs
- Communications
- Workflow dates

with the episode.

### 3. Explicit closure record

Store:

- ClosedAt
- ClosedByUserId
- Closure reason
- Final outcome

at the episode level.

### 4. Automatic 60-day processing

Implement a scheduled/background process for detecting episodes that reach their 60-day boundary.

### 5. SLA monitoring

Calculate and surface:

- Referral → SOC 48-hour SLA
- SOC → OASIS 48-hour SLA
- OASIS → POC 24-hour SLA
- Discharge → Discharge OASIS 5-day SLA

### 6. Better episode history

Provide a patient history screen:

```text
Patient
 ├── Current Episode
 └── Previous Episodes
       ├── Episode #1
       ├── Episode #2
       └── ...
```

---

# 51. Project Documentation

Existing documentation:

```text
ARCHITECTURE.md
BUILD_AND_DATABASE.md
README.md
```

This README is intended to be the main project-level reference.

---

# 52. Quick Reference

## Start API

```bash
dotnet run --project CCAP.API
```

## Start Web

```bash
dotnet run --project CCAP.Web
```

## Build

```bash
dotnet build CCAP.sln
```

## Restore

```bash
dotnet restore
```

## EF migration update

```bash
dotnet ef database update \
  --project CCAP.Infrastructure \
  --startup-project CCAP.API
```

## Main workflow

```text
Referral
→ Pre-Intake
→ Insurance
→ SOC
→ Admission
→ Ongoing Care
→ Final Outcome
→ Discharge
→ Patient Journey Closed
→ Archive
```

## New episode

```text
Archived/previous patient
→ New Referral
→ New care episode
```

---

# 53. Final Design Principle

CCAP is a **patient-care coordination and workflow system**.

The application should make it obvious:

> **What is the current patient-care episode?**

> **What needs to be completed next?**

> **Who is responsible for the coordination action?**

> **What has already been verified?**

> **What is the final outcome?**

> **Has the patient journey been closed?**

> **Can the same patient return for another episode without losing historical information?**

The system should preserve the operational concepts of the Excel workflow while providing structured data, permissions, auditability, workflow progression, and a clear patient journey from referral through discharge and closure.

## List, Search, Filter, Sort, and Pagination

The current web application standardizes list behavior across scalable tables:

- Patients and Archived Patients: server-side search, status filtering, sorting, and pagination.
- Draft Referrals: server-side search, sorting, and pagination.
- Users: search, role/discipline/status filters, sorting, and pagination.
- Roles: search/status filters, sorting, and pagination.
- Lookup Options: group/search/status filters, sorting, and pagination.
- Service / Order Types: search/status filters, sorting, and pagination.
- Patient workflow Communications: search, contact type/method filters, and pagination.
- Patient workflow Audit Log: search, action filter, and pagination.
- Patient workflow Care Logs: search and pagination.
- Patient Documents: search, category/status filters, and pagination.

Pagination defaults to 20 rows with 50 and 100 row options. Changing a search/filter resets the list to page 1. Patient and referral-draft lists perform the main filtering, sorting, counting, and paging in the API/database layer rather than loading the complete dataset into the browser.

## Calendar database integration

The Scheduling Calendar no longer uses hard-coded sample schedule entries. The Blazor calendar loads schedule records from the API endpoint `GET /api/scheduling/calendar?startDate={startDate}&endDate={endDate}`.

The endpoint reads persisted `Visits` through the Clean Architecture application/repository layers and returns patient, scheduled date/time, clinician, status, completion date, and notes. Month, Week, Day, Today, and previous/next navigation reload the visible date range from the API.

The schedule import button is intentionally disabled for this phase. The next scheduling phase will implement the `CLINICAL WORK SCHED` Excel import, duplicate detection, and the explicit duplicate review flow where checked entries are overwritten and unchecked entries keep the existing schedule.


### Schedule date parsing
The schedule importer prefers native Excel date values, supports common explicit date formats, and rejects ambiguous text dates instead of guessing.

## Calendar import behavior
Imported schedules are now persisted even when the patient is not yet present/matched in CCAP. The Visits record stores the imported patient/clinician names and schedule metadata, while PatientId and ClinicianId are optional links when CCAP records are available. Matching remains useful for linking a schedule to a patient but is not an import prerequisite.

The schedule review modal closes before the SweetAlert loading dialog opens, so success/error alerts cannot appear behind the review modal. Close and Cancel explicitly clear the pending review when the import is not running. Mapping warnings use Proceed with Caution and do not block saving.
