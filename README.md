# CCAP Development Documentation

> **Document purpose:** This is the development documentation for the actual CCAP codebase
>
> It consolidates and corrects the existing repository Markdown documentation against the source code. It describes what is implemented, how the layers communicate, how authentication/authorization currently works, how the database is modeled, and which UI areas are currently API-backed versus still prototype/static.
>

---

# 1. System Overview

CCAP is structured as a Clean Architecture application with five projects:

```text
CCAP.Domain
      ↑
CCAP.Application
      ↑
CCAP.Infrastructure
      ↑
CCAP.API
      ↑
CCAP.Web
```

The intended dependency direction is:

```text
Presentation
 ├── CCAP.Web
 └── CCAP.API
        ↓
   Application
        ↓
     Domain

Infrastructure implements Application abstractions
and references Domain/Application.
```

The main runtime architecture is:

```text
Browser
   │
   ▼
CCAP.Web
Blazor Interactive Server
   │
   │ HTTP + JWT Bearer
   ▼
CCAP.API
ASP.NET Core Web API
   │
   ▼
MediatR / Application
   │
   ▼
Infrastructure
Repositories + EF Core
   │
   ▼
SQL Server
```

---

# 2. Project Responsibilities

## 2.1 CCAP.Domain

Location:

```text
CCAP.Domain/
```

Responsibilities:

- Domain entities
- Domain state transitions
- Domain enums
- Domain-level business rules

The Domain project does not depend on:

- EF Core
- ASP.NET Core
- HTTP
- Blazor
- SQL Server

### Current entities

```text
ApplicationUser
Role
Permission
RolePermission
Discipline

Patient
Referral
CallNote
Assessment
ComplianceRecord
PatientTask
Activity
Visit
ServiceType
PatientServiceOrder
```

### Current enums

```text
PatientStatus
ReferralStatus
PatientTaskStatus
```

---

# 3. CCAP.Application

Location:

```text
CCAP.Application/
```

Responsibilities:

- CQRS commands and queries
- MediatR handlers
- DTOs
- Persistence abstractions
- Identity/security abstractions
- Application use cases

The Application layer does not directly use:

```text
DbContext
SQL Server
JWT implementation
ASP.NET controllers
Blazor
```

Instead, it depends on interfaces such as:

```text
IUserRepository
IRoleRepository
IPatientRepository
IReferralRepository
ICallNoteRepository
IServiceTypeRepository
IAdminLookupRepository
IUnitOfWork

IPasswordHasher
IJwtService
```

Infrastructure implements these interfaces.

---

# 4. CCAP.Infrastructure

Location:

```text
CCAP.Infrastructure/
```

Responsibilities:

- EF Core
- SQL Server
- `AppDbContext`
- repositories
- Unit of Work
- password hashing implementation
- JWT implementation
- database seeding
- EF Core design-time factory
- EF Core migrations

The important separation is:

```text
Application
    │
    ├── IUserRepository
    ├── IPatientRepository
    ├── IRoleRepository
    ├── IJwtService
    └── IPasswordHasher
          ▲
          │ implemented by
          │
Infrastructure
```

---

# 5. CCAP.API

Location:

```text
CCAP.API/
```

Responsibilities:

- HTTP API
- Controller routing
- JWT authentication
- ASP.NET Core authorization
- Permission policies
- Swagger in development
- Database initialization/seeding

Current controllers:

```text
AuthController
UsersController
AdminController
PatientsController
```

The API is the **final security boundary**.

The Web UI can hide buttons or pages, but an API endpoint must independently enforce authorization.

---

# 6. CCAP.Web

Location:

```text
CCAP.Web/
```

Technology:

```text
Blazor Interactive Server
```

Responsibilities:

- User interface
- Routing
- Login
- Authentication state
- JWT persistence
- API client
- User management UI
- Role/permission UI
- Patient UI
- Referral intake prototype
- Patient workflow prototype
- Dashboard UI

The Web application does not directly access SQL Server.

It communicates with:

```text
CCAP.API
```

---

# 7. Runtime Data Flow

For an API-backed operation, the normal flow is:

```text
Razor component
      │
      ▼
Web service
      │
      ▼
CcapApiClient
      │
      ▼
TokenStore
      │
      ▼
Authorization: Bearer <JWT>
      │
      ▼
CCAP.API controller
      │
      ▼
Permission policy
      │
      ▼
MediatR command/query
      │
      ▼
Repository abstraction
      │
      ▼
Infrastructure repository
      │
      ▼
AppDbContext
      │
      ▼
SQL Server
```

The response travels back through the same layers:

```text
SQL Server
   ↓
EF Core
   ↓
Repository
   ↓
Application handler
   ↓
DTO
   ↓
Controller
   ↓
JSON
   ↓
CcapApiClient
   ↓
Web service
   ↓
Razor component
```

---

# 8. Authentication Architecture

Authentication is JWT-based.

The current flow is:

```text
CCAP.Web /login
      │
      │ POST /api/auth/login
      ▼
CCAP.API AuthController
      │
      ▼
LoginCommand
      │
      ▼
LoginCommandHandler
      │
      ├── IUserRepository
      ├── IPasswordHasher
      └── IJwtService
      │
      ▼
SQL Server user/role/permissions
      │
      ▼
JWT
      │
      ▼
CCAP.Web AuthenticationService
      │
      ▼
TokenStore
      │
      ├── memory
      └── ProtectedLocalStorage
```

---

# 9. Login Processing

The login endpoint is:

```http
POST /api/auth/login
```

Implemented by:

```text
CCAP.API/Controllers/AuthController.cs
```

The endpoint is anonymous:

```csharp
[AllowAnonymous]
[HttpPost("login")]
```

The controller sends the request through MediatR:

```text
AuthController
    ↓
LoginCommand
    ↓
LoginCommandHandler
```

---

# 10. LoginCommandHandler

The handler performs:

1. Find user by email.
2. Ensure user exists.
3. Ensure user is active.
4. Verify password hash.
5. Generate JWT.
6. Return login result.

Invalid login returns:

```text
Invalid email or password.
```

The handler does not expose whether the email exists separately from whether the password is wrong.

---

# 11. JWT Contents

`JwtService` creates claims for:

```text
sub
email
NameIdentifier
Name
Role
permission
```

The permission claims come from:

```text
user
  ↓
Role
  ↓
RolePermissions
  ↓
Permission
```

For example:

```text
Administrator
    ↓
users.view
users.manage
roles.view
roles.manage
patients.view
patients.manage
referrals.view
referrals.manage
```

The JWT also contains:

```text
Issuer
Audience
Expiration
Signature
```

The signing algorithm is:

```text
HMAC SHA-256
```

---

# 12. API JWT Validation

`CCAP.API/Program.cs` configures:

```text
JwtBearerDefaults.AuthenticationScheme
```

The API validates:

- signing key
- issuer
- audience
- lifetime

with a one-minute clock skew.

The API therefore does not trust a JWT merely because the Web application created it.

The token must pass API-side cryptographic validation.

---

# 13. Authentication State in Blazor

The Web application uses:

```text
CcapAuthenticationStateProvider
```

located at:

```text
CCAP.Web/Features/Authentication/State/CcapAuthenticationStateProvider.cs
```

It inherits:

```csharp
AuthenticationStateProvider
```

It converts JWT claims into a:

```text
ClaimsPrincipal
```

This allows components such as:

```razor
<AuthorizeView>
```

to use the current user.

---

# 14. TokenStore

Location:

```text
CCAP.Web/Features/Authentication/Services/TokenStore.cs
```

`TokenStore` is registered as:

```csharp
AddScoped<TokenStore>()
```

It maintains the token in memory and persists it through:

```text
ProtectedLocalStorage
```

The storage key is:

```text
ccap.auth.token
```

Important behavior:

```text
GetAsync()
```

does not access JavaScript or browser storage.

It returns the current in-memory token.

This avoids the previous prerendering problem:

```text
JavaScript interop calls cannot be issued at this time.
```

---

# 15. Authentication Persistence

The refresh sequence is:

```text
Browser refresh
      ↓
new Blazor circuit
      ↓
CcapAuthenticationStateProvider
      ↓
Routes.razor
      ↓
LoadPersistedAsync()
      ↓
TokenStore.LoadPersistedAsync()
      ↓
ProtectedLocalStorage
      ↓
JWT restored
      ↓
ClaimsPrincipal recreated
      ↓
authorized route rendering
```

The token is loaded from browser storage only after the interactive Blazor circuit exists.

---

# 16. Routes and Authorization

`Routes.razor` provides:

```text
CascadingAuthenticationState
Router
CcapAuthorizeRouteView
```

The route view checks:

```text
CcapAuthorizeAttribute
```

on the page.

The current implementation supports:

```text
Policy
Roles
```

Example:

```razor
@attribute [CcapAuthorize]
```

means the page requires an authenticated user.

A policy can be specified as:

```razor
@attribute [CcapAuthorize(Policy = "patients.view")]
```

The current `CcapAuthorizeRouteView` checks whether the authenticated user's claims contain:

```text
permission = patients.view
```

---

# 17. Important Current Authorization Distinction

The current code has **two authorization mechanisms**.

## Web authorization

Implemented by:

```text
CcapAuthorizeAttribute
CcapAuthorizeRouteView
AuthenticationStateProvider
```

This controls what the Blazor UI allows the current user to navigate to.

## API authorization

Implemented by:

```text
ASP.NET Core JWT authentication
PermissionAuthorizationHandler
PermissionPolicies
[Authorize(Policy = "...")]
```

This is the real security boundary.

The API must remain protected even if someone bypasses the UI.

---

# 18. Current Web Page Authorization State

The following pages currently use:

```razor
@attribute [CcapAuthorize]
```

rather than a specific permission policy:

```text
/dashboard
/admin/users
/admin/roles
/patients
/referrals
/tracker/patient
/
```

Therefore the current Web route layer primarily checks **authentication** for these pages.

It does not currently assign a specific permission policy to each of these pages.

The API still performs its own permission checks where configured.

---

# 19. API Permission Model

Permission constants are defined in:

```text
CCAP.API/Authorization/PermissionPolicies.cs
```

Current permissions:

```text
users.view
users.manage

roles.view
roles.manage

patients.view
patients.manage

referrals.view
referrals.manage
```

Each policy is connected to:

```text
PermissionRequirement
```

and:

```text
PermissionAuthorizationHandler
```

The handler checks whether the JWT contains:

```text
permission
```

with the required permission code.

---

# 20. Permission Evaluation

For:

```csharp
[Authorize(Policy = PermissionPolicies.UsersManage)]
```

the API executes:

```text
JWT
 ↓
ClaimsPrincipal
 ↓
permission claim
 ↓
PermissionRequirement
 ↓
PermissionAuthorizationHandler
 ↓
users.manage?
```

If the permission exists:

```text
authorized
```

Otherwise:

```text
403 Forbidden
```

---

# 21. Role and Permission Database Model

The security relationship is:

```text
ApplicationUser
      │
      │ RoleId
      ▼
Role
      │
      │ RoleId
      ▼
RolePermission
      │
      │ PermissionId
      ▼
Permission
```

This is a many-to-many relationship between:

```text
Role
```

and:

```text
Permission
```

through:

```text
RolePermission
```

---

# 22. Current Seeded Roles

The database seeder creates:

```text
Administrator
Care Coordinator
Clinician
Scheduler
```

Initial permission assignments:

## Administrator

All permissions.

## Care Coordinator

```text
users.view
roles.view
patients.view
patients.manage
referrals.view
referrals.manage
```

## Clinician

```text
patients.view
patients.manage
referrals.view
```

## Scheduler

```text
patients.view
referrals.view
referrals.manage
```

---

# 23. Development Administrator

The seeder creates:

```text
Email:
admin@ccap.local

Password:
Admin123!
```

This is a development account.

It must not be treated as a production credential.

---

# 24. Dynamic Logged-in Profile

The topbar obtains the current user from:

```text
AuthenticationStateProvider
```

It reads:

```text
ClaimTypes.Name
ClaimTypes.Role
```

The displayed profile name is therefore based on the authenticated JWT.

The topbar is no longer dependent on a hard-coded user identity.

The current implementation does not yet load a separate profile record from the API for the topbar.

It uses JWT claims.

---

# 25. Logout

The current logout process is:

```text
UserProfileMenu
      ↓
TokenStore.DeleteAsync()
      ↓
ProtectedLocalStorage.DeleteAsync()
      ↓
CcapAuthenticationStateProvider.NotifyLogoutAsync()
      ↓
Anonymous ClaimsPrincipal
      ↓
/login
```

The API does not have a server-side logout endpoint because the current JWT model is stateless.

Deleting the token from the Web session is the current logout mechanism.

---

# 26. Login Layout vs Main Layout

The login page uses:

```razor
@layout LoginLayout
```

Therefore:

```text
/login
```

does not use:

```text
MainLayout
```

Authenticated pages use:

```text
MainLayout
```

which contains:

```text
NavMenu
Topbar
page content
```

The layout flow is:

```text
Login
 ↓
LoginLayout

Authenticated page
 ↓
MainLayout
 ├── NavMenu
 ├── Topbar
 └── Body
```

---

# 27. CCAP API Client

Location:

```text
CCAP.Web/Features/Authentication/Services/CcapApiClient.cs
```

The client obtains the current token from the scoped `TokenStore`.

It adds:

```http
Authorization: Bearer <JWT>
```

to outgoing API requests.

Supported operations currently include:

```text
GET
GET JSON
POST JSON
PUT JSON
PATCH
DELETE
```

This centralizes bearer-token handling.

---

# 28. Current Web API Service Layer

The Web services currently use `CcapApiClient`.

### UserServices

Handles:

```text
GET /api/users
GET /api/users/{id}
POST /api/users
PUT /api/users/{id}
PATCH /api/users/{id}/activate
PATCH /api/users/{id}/deactivate
DELETE /api/users/{id}
```

### RoleService

Handles:

```text
GET /api/admin/roles
GET /api/admin/permissions
GET /api/admin/roles/{roleId}
PUT /api/admin/roles/{roleId}/permissions
```

### AdminLookupService

Handles:

```text
GET /api/admin/disciplines
```

### PatientService

Handles:

```text
GET /api/patients
```

### PatientClinicalService

Handles:

```text
GET /api/patients/service-types
```

---

# 29. API Endpoint Map

## Authentication

```http
POST /api/auth/login
```

Authorization:

```text
Anonymous
```

---

## Users

```http
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
PATCH  /api/users/{id}/activate
PATCH  /api/users/{id}/deactivate
```

Permissions:

```text
users.view
users.manage
```

---

## Administration

```http
GET /api/admin/roles
GET /api/admin/roles/{roleId}
GET /api/admin/permissions
PUT /api/admin/roles/{roleId}/permissions
GET /api/admin/disciplines
```

Permissions:

```text
roles.view
roles.manage
```

`GET /api/admin/disciplines` currently requires authentication but not a specific permission policy.

---

## Patients

```http
GET  /api/patients
GET  /api/patients/service-types

POST /api/patients/{patientId}/call-notes
POST /api/patients/{patientId}/complete-care
POST /api/patients/{patientId}/archive
```

Current endpoint policies:

```text
GET /api/patients
    Controller-level authentication only

GET /api/patients/service-types
    patients.view

POST call-notes
    patients.manage

POST complete-care
    patients.manage

POST archive
    patients.manage
```

The absence of a `patients.view` policy on `GET /api/patients` is an important current implementation detail.

---

# 30. User Management Logic

The User Management page is:

```text
/admin/users
```

The UI loads:

```text
Roles
Disciplines
Users
```

The role and discipline dropdowns are API-backed.

The user table supports:

```text
Search
Role filter
Discipline filter
Active/inactive filter
```

Statistics are calculated in the Web page:

```text
Total Users
Active Users
Inactive Users
Administrators
```

---

# 31. Creating a User

The UI collects:

```text
Employee No.
Email
First Name
Last Name
Mobile No.
Temporary Password
Role
Discipline
```

The Web calls:

```http
POST /api/users
```

The Application handler:

1. Checks email uniqueness.
2. Checks employee number uniqueness.
3. Finds the role.
4. Requires the role to be active.
5. Creates `ApplicationUser`.
6. Hashes the password.
7. Saves the user.
8. Returns `UserDto`.

Passwords are not stored directly.

---

# 32. Updating a User

The Web sends:

```http
PUT /api/users/{id}
```

The handler:

1. Finds the user.
2. Finds the requested role.
3. Requires the role to be active.
4. Updates user properties.
5. Saves changes.

Activation state is currently handled separately by:

```http
PATCH /api/users/{id}/activate
PATCH /api/users/{id}/deactivate
```

---

# 33. Deleting a User

The API supports:

```http
DELETE /api/users/{id}
```

The current handler removes the user from the database.

This is a true delete, not merely a deactivate.

The UI's main management flow favors activation/deactivation for account state, but the API still exposes deletion.

---

# 34. Role Management

The page is:

```text
/admin/roles
```

It loads:

```text
Roles
Permissions
Role permission assignments
```

The role table calculates statistics such as:

```text
Total roles
Total permissions assigned
Assigned users
Custom roles
```

---

# 35. Managing Role Permissions

Selecting a role opens the permission modal.

The Web loads:

```text
All permissions
Current role permissions
```

The user selects permission IDs.

The Web sends:

```http
PUT /api/admin/roles/{roleId}/permissions
```

with:

```json
{
  "PermissionIds": [
    "..."
  ]
}
```

The Application handler:

1. Finds the role.
2. Requires it to be active.
3. Replaces the existing `RolePermission` records.
4. Validates the supplied permission IDs against the Permissions table.
5. Adds distinct valid permissions.
6. Saves changes.

---

# 36. Important JWT Permission Behavior

Changing a role's permissions does **not automatically modify an already-issued JWT**.

The existing JWT contains the permissions that existed when it was created.

Therefore the current documented workflow is:

```text
Change role permissions
       ↓
Log out
       ↓
Log in again
       ↓
New JWT generated
       ↓
New permission claims
```

This is explicitly reflected in the existing test documentation.

---

# 37. Patient Domain Model

A `Patient` contains:

```text
PatientId
MRN
FirstName
MiddleName
LastName
DateOfBirth
PrimaryDiagnosis
Address
PhoneNumber
Status

CoordinatorId
ClinicianId
SocDate

CareCompletedAt
FinalizedByUserId
FinalStatus

ArchivedAt
ArchivedByUserId
```

Patient status values:

```text
Active
OnHold
Completed
Cancelled
Archived
```

---

# 38. Patient Relationships

A patient can have:

```text
Referrals
CallNotes
Assessments
ComplianceRecords
PatientTasks
Activities
Visits
PatientServiceOrders
```

The patient also references:

```text
Coordinator
Clinician
```

which are `ApplicationUser` records.

---

# 39. Patient Lifecycle in the Domain

The implemented domain methods include:

```text
CompleteCare()
Archive()
```

`CompleteCare()` requires:

```text
Patient.Status == Active
```

and requires a final status.

It changes:

```text
Status = Completed
CareCompletedAt = current UTC time
FinalStatus = supplied status
FinalizedByUserId = supplied user
```

`Archive()` requires:

```text
Patient.Status == Completed
```

and changes:

```text
Status = Archived
ArchivedAt = current UTC time
ArchivedByUserId = supplied user
```

This means the domain itself prevents archiving an active patient.

---

# 40. Patient List

The page is:

```text
/patients
```

It calls:

```text
PatientService.GetPatientsAsync()
```

which calls:

```http
GET /api/patients
```

The Application query retrieves patients through:

```text
IPatientRepository.GetAllAsync()
```

The repository includes:

```text
Clinician
```

and sorts by:

```text
LastName
FirstName
```

The Application maps the entity to:

```text
PatientListItemDto
```

---

# 41. Patient Call Notes

The domain entity:

```text
CallNote
```

contains:

```text
PatientId
RecordedByUserId
CallDate
Subject
Notes
Outcome
```

The API endpoint is:

```http
POST /api/patients/{patientId}/call-notes
```

Permission:

```text
patients.manage
```

The handler:

1. Verifies patient exists.
2. Creates `CallNote`.
3. Adds it through `ICallNoteRepository`.
4. Saves.
5. Returns the new CallNote ID.

---

# 42. Important Current Call-Note Limitation

The API command currently accepts:

```text
RecordedByUserId
```

from the command.

The handler does not currently extract the logged-in user's ID from:

```text
HttpContext.User
```

or:

```text
ClaimsPrincipal
```

itself.

Therefore, the caller must currently supply the user ID.

For a production security hardening pass, the actor identity should be derived server-side from the validated JWT rather than trusting a client-provided `RecordedByUserId`.

---

# 43. Complete Care

The API endpoint:

```http
POST /api/patients/{patientId}/complete-care
```

requires:

```text
patients.manage
```

The handler finds the patient and calls:

```csharp
patient.CompleteCare(...)
```

The domain enforces the state transition.

---

# 44. Archive Patient

The API endpoint:

```http
POST /api/patients/{patientId}/archive
```

requires:

```text
patients.manage
```

The handler calls:

```csharp
patient.Archive(...)
```

The domain prevents archiving unless care is already completed.

---

# 45. Referral Domain

The `Referral` entity contains:

```text
ReferralId
ReferralNumber
PatientId
ReferralDate
Status
Source
Priority
AssignedUserId
AssignedAt
```

Referral statuses:

```text
Received
UnderReview
Assigned
Accepted
ConvertedToPatient
Closed
```

Domain methods:

```text
Assign()
MarkUnderReview()
ConvertToPatient()
```

---

# 46. Referral Repository

Infrastructure contains:

```text
ReferralRepository
```

It supports:

```text
GetByIdAsync
AddAsync
Update
```

It includes:

```text
Patient
AssignedUser
```

when loading a referral.

---

# 47. Important Current Referral Limitation

The Application project contains a referral repository abstraction, and the Domain contains a Referral entity.

However, the current API controller set does **not** contain a ReferralController.

The Web `/referrals` page is currently a wizard UI and does not submit a referral to the API.

Therefore the referral workflow is currently **partially implemented at the backend/domain level but not connected end-to-end from the Web UI**.

---

# 48. Referral Intake UI

The Web route is:

```text
/referrals
```

The wizard contains components for:

```text
UploadStep
ReferralInformationStep
PatientInformationStep
AssignmentStep
ReviewStep
WizardHeader
WizardProgress
```

The current page manages:

```text
CurrentStep
```

and supports:

```text
Next
Previous
```

The final button is:

```text
Create Referral
```

but the current page does not call an API command.

Therefore it is currently a UI prototype.

---

# 49. Patient Workflow UI

The route is:

```text
/tracker/patient
```

There is also a previously intended parameterized route:

```text
/tracker/patient/{id:int}
```

but the current page has that route commented out and uses:

```text
/tracker/patient
```

The current patient workflow page displays a static patient header and workflow state.

---

# 50. Patient Workflow Tabs

Current tabs:

```text
Overview
Insurance
Clinical
Compliance
Documents
Communication
Timeline
Audit Log
```

The selected tab is stored in:

```text
SelectedTab
```

and switched locally.

The workflow page contains:

```text
PatientHeader
WorkflowTabs
WorkflowProgress
NextActionCard
KeyInformationCard
RecentActivityCard
PatientSummaryBar
```

---

# 51. Patient Workflow Stages

The current static workflow contains:

```text
1. Referral
2. Insurance
3. SOC Scheduled
4. Admission
5. Visits
6. Recertification
7. Discharge
```

The current sample state is:

```text
Referral       Completed
Insurance      Completed
SOC Scheduled  Current
Admission      Pending
Visits         Pending
Recertification Pending
Discharge      Pending
```

This represents the intended visual workflow but is not currently loaded from a Patient Workflow API.

---

# 52. Patient Workflow Static Data

The current page contains sample values such as:

```text
John Michael Smith
MRN: 123456789
Referral: REF-2026-000123
Coordinator: Jennifer LVN
Branch: Main Office
```

These are hard-coded in the current Razor component.

Therefore the patient workflow page is currently a prototype and is **not yet dynamically bound to the selected patient from the database**.

---

# 53. Clinical Service Types

The backend supports database-driven clinical service types.

The `ServiceType` entity contains:

```text
ServiceTypeId
Code
Name
Icon
CssClass
IsActive
```

Seeded service types:

```text
SN   Skilled Nursing
PT   Physical Therapy
OT   Occupational Therapy
ST   Speech Therapy
HHA  Home Health Aide
```

The API endpoint is:

```http
GET /api/patients/service-types
```

Permission:

```text
patients.view
```

The Web `PatientClinicalService` calls this endpoint.

---

# 54. PatientServiceOrder

The database model supports patient service orders:

```text
Patient
   ↓
PatientServiceOrder
   ↓
ServiceType
```

Fields include:

```text
Status
Frequency
Duration
IsPrimaryDiscipline
```

The database enforces uniqueness for:

```text
PatientId + ServiceTypeId
```

The current API controller does not yet expose CRUD endpoints for `PatientServiceOrder`.

---

# 55. Assessment

The domain contains:

```text
Assessment
```

Fields:

```text
AssessmentId
PatientId
CompletedByUserId
CompletedAt
Status
Notes
```

The constructor creates an assessment with:

```text
Status = Completed
CompletedAt = current UTC time
```

The current API controller does not expose assessment CRUD endpoints.

---

# 56. ComplianceRecord

The domain contains:

```text
ComplianceRecord
```

Fields:

```text
ComplianceRecordId
PatientId
RequirementCode
IsCompleted
CompletedAt
CompletedByUserId
Notes
```

The domain method:

```text
Complete(userId)
```

sets:

```text
IsCompleted = true
CompletedAt = current UTC time
CompletedByUserId = userId
```

The current API controller does not expose compliance endpoints.

---

# 57. PatientTask

The domain contains:

```text
PatientTask
```

with:

```text
Pending
InProgress
Completed
Cancelled
```

Supported domain operations:

```text
Assign()
Start()
Complete()
Cancel()
```

The domain prevents:

```text
Cancelled → Started
Cancelled → Completed
Completed → Cancelled
```

The current API does not expose PatientTask endpoints.

---

# 58. Activity

The `Activity` entity represents patient-related activity:

```text
PatientId
PerformedByUserId
ActivityDate
ActivityType
Title
Description
```

It is linked to a patient.

The current API does not expose Activity endpoints.

---

# 59. Visit

The `Visit` entity contains:

```text
PatientId
ClinicianId
ScheduledDate
CompletedDate
Status
Notes
```

A visit starts as:

```text
Scheduled
```

and can be completed using:

```text
Complete(notes)
```

The current API does not expose Visit endpoints.

---

# 60. Database Model

`AppDbContext` currently contains:

```text
ApplicationUsers
Roles
Permissions
RolePermissions
Disciplines

Patients
Referrals
CallNotes
Assessments
ComplianceRecords
PatientTasks
Activities
Visits

ServiceTypes
PatientServiceOrders
```

---

# 61. Main Security Relationships

```text
Roles
  │
  └── RolePermissions ── Permissions

ApplicationUsers
  │
  ├── Role
  └── Discipline
```

---

# 62. Main Patient Relationships

```text
Patient
 ├── Referrals
 ├── CallNotes
 ├── Assessments
 ├── ComplianceRecords
 ├── PatientTasks
 ├── Activities
 ├── Visits
 └── PatientServiceOrders
          │
          └── ServiceType
```

Patient references:

```text
Coordinator → ApplicationUser
Clinician   → ApplicationUser
```

---

# 63. EF Core Relationship Details

## Role

One Role has many:

```text
ApplicationUsers
RolePermissions
```

## Permission

One Permission has many:

```text
RolePermissions
```

## RolePermission

Many-to-many bridge:

```text
Role ↔ Permission
```

with unique:

```text
RoleId + PermissionId
```

## ApplicationUser

References:

```text
Role
Discipline
```

## Patient

References:

```text
Coordinator
Clinician
```

and owns collections for patient activity.

---

# 64. EF Core Delete Behavior

Security relationships generally use restricted deletes where user references must be preserved.

For example:

```text
ApplicationUser → Role
ApplicationUser → Discipline
Patient → Coordinator
Patient → Clinician
Referral → Patient
Referral → AssignedUser
```

use restricted deletion.

Patient child records such as:

```text
CallNotes
Assessments
ComplianceRecords
PatientTasks
Activities
Visits
PatientServiceOrders
```

are configured with cascade behavior from the Patient where appropriate.

---

# 65. Database Seeder

Location:

```text
CCAP.Infrastructure/Seed/DatabaseSeeder.cs
```

The seeder is intended to be idempotent.

It creates or ensures:

```text
Permissions
Roles
RolePermissions
Disciplines
ServiceTypes
Development Administrator
```

It checks for existing records rather than blindly inserting duplicates.

---

# 66. Database Seeder Permissions

The initial permissions are:

```text
users.view
users.manage
roles.view
roles.manage
patients.view
patients.manage
referrals.view
referrals.manage
```

---

# 67. Database Seeder Disciplines

The initial disciplines are:

```text
RN   Registered Nurse
LVN  Licensed Vocational Nurse
PT   Physical Therapy
OT   Occupational Therapy
ST   Speech Therapy
HHA  Home Health Aide
```

---

# 68. Database Seeder Service Types

The initial service types are:

```text
SN
PT
OT
ST
HHA
```

Each contains UI metadata:

```text
Icon
CssClass
```

This allows clinical service types to be driven by database data rather than hard-coded Razor values.

---

# 69. EF Core Migrations

Current migrations include:

```text
InitialCCAPSchema
AddSecurityAndPermissions
```

Migrations are located in:

```text
CCAP.Infrastructure/Migrations/
```

They are part of the source-controlled database schema.

They should not be added to `.gitignore`.

---

# 70. AppDbContextFactory

Location:

```text
CCAP.Infrastructure/Persistence/AppDbContextFactory.cs
```

Its purpose is design-time creation of:

```text
AppDbContext
```

for EF Core tooling.

It is not required as part of normal runtime request processing.

The runtime API uses:

```text
AddDbContext<AppDbContext>()
```

---

# 71. Database Bootstrap Scripts

The repository also contains:

```text
database/CCAP-FreshDatabase.sql
database/CCAP-Admin-Upgrade.sql
database/CCAP-Admin-Setup.sql
```

## CCAP-FreshDatabase.sql

Creates the current database schema from scratch.

Use it for a completely fresh SQL Server database when appropriate.

## CCAP-Admin-Upgrade.sql

Adds the security/admin tables and relationships to an existing CCAP database.

It was specifically intended to address cases such as:

```text
Invalid object name 'Permissions'
```

## CCAP-Admin-Setup.sql

Adds service-related database structures to an existing CCAP database.

---

# 72. EF Migrations vs SQL Scripts

The intended long-term schema source is:

```text
EF Core migrations
```

The SQL scripts are useful for:

```text
fresh database bootstrap
existing database upgrade
recovery/testing
```

For production schema changes, review and deploy the appropriate EF migration/script rather than relying on ad-hoc manual table creation.

---

# 73. Dashboard

The Web route is:

```text
/dashboard
```

The current Dashboard page is a **static UI prototype**.

It contains hard-coded sample data for:

```text
Work statistics
Tasks
Recent referrals
Upcoming visits
Announcements
```

Examples include sample users/patients such as:

```text
John Smith
Maria Cruz
James Brown
Jennifer RN
```

The current Dashboard does not call a Dashboard API service.

Therefore the Dashboard is currently not dynamically connected to the database.

---

# 74. Dashboard Intended Architecture

The intended future architecture should be:

```text
Dashboard.razor
      ↓
DashboardService
      ↓
CcapApiClient
      ↓
CCAP.API
      ↓
Dashboard query
      ↓
Application
      ↓
Repositories
      ↓
Database
```

But that service/controller/query does not currently exist in the provided codebase.

---

# 75. Referral UI vs Backend

Current state:

```text
Referral domain              EXISTS
Referral repository          EXISTS
Referral repository interface EXISTS

Referral API controller     NOT PRESENT
Referral CQRS commands       NOT PRESENT in current Application feature list
Referral Web API submission  NOT PRESENT
Referral wizard UI           EXISTS
```

Therefore `/referrals` should be considered a prototype until the API workflow is implemented.

---

# 76. Patient Workflow UI vs Backend

Current state:

```text
Workflow models             EXISTS
Workflow components         EXISTS
Workflow page               EXISTS
Clinical service API client EXISTS

Workflow API endpoints      NOT PRESENT
Dynamic patient loading     NOT PRESENT
Dynamic workflow state      NOT PRESENT
Dynamic tab persistence     NOT PRESENT
```

Therefore the current workflow is primarily a presentation prototype.

---

# 77. Communication Tab

The backend has a real:

```text
CallNote
```

entity and API command.

However, the current Patient Workflow `CommunicationTab` is part of the UI component set and the workflow page itself does not currently wire that tab to the call-note API.

The backend capability exists, but the end-to-end workflow connection still needs to be completed.

---

# 78. Compliance Tab

The backend has:

```text
ComplianceRecord
```

and domain behavior for completion.

The current Web UI contains:

```text
ComplianceTab
```

but there is no current Compliance API controller or Application command/query set exposed through the Web.

Therefore it is currently a UI/domain foundation rather than a complete end-to-end feature.

---

# 79. Documents Tab

The current Web application contains:

```text
DocumentsTab
```

but there is no corresponding document entity/repository/controller in the current backend code.

It should therefore be treated as UI/prototype functionality until a document storage/API subsystem is implemented.

---

# 80. Insurance Tab

The current Web application contains:

```text
InsuranceTab
```

but there is no current Insurance entity/controller/API feature in the supplied backend.

It is currently a presentation layer component.

---

# 81. Timeline and Audit Log

The Web workflow contains:

```text
TimelineTab
AuditLogTab
```

The Domain contains:

```text
Activity
```

but there is currently no general Activity API controller or Audit Log subsystem exposed by the API.

Therefore the current tabs are not fully database-backed.

---

# 82. Current Feature Matrix

| Feature | Web UI | API | Application | Domain/DB | End-to-end |
|---|---:|---:|---:|---:|---:|
| Login | Yes | Yes | Yes | Yes | Yes |
| JWT authentication | Yes | Yes | Yes | Yes | Yes |
| User management | Yes | Yes | Yes | Yes | Yes |
| Role management | Yes | Yes | Yes | Yes | Yes |
| Permission management | Yes | Yes | Yes | Yes | Yes |
| Patient list | Yes | Yes | Yes | Yes | Yes |
| Patient call notes | Component foundation | Yes | Yes | Yes | Partial Web wiring |
| Complete care | UI foundation | Yes | Yes | Yes | Partial Web wiring |
| Archive patient | UI foundation | Yes | Yes | Yes | Partial Web wiring |
| Service types | Service/UI support | Yes | Yes | Yes | Yes |
| Referral intake | Yes | No | Partial domain/repository | Yes | No |
| Dashboard | Yes | No | No | No dashboard query | No |
| Patient workflow | Yes | No | No workflow API | Partial domain | No |
| Compliance | Yes | No | No | Yes | No |
| Documents | Yes | No | No | No document backend | No |
| Insurance | Yes | No | No | No insurance backend | No |
| Timeline | Yes | No | No | Partial Activity entity | No |
| Audit Log | Yes | No | No | No audit subsystem | No |
| Visits | UI model support | No | No | Yes | No |
| Patient tasks | UI model support | No | No | Yes | No |
| Assessments | UI/domain support | No | No | Yes | No |

---

# 83. Important Current Codebase Limitations

The following are not assumptions; they are visible in the supplied code.

## 83.1 Dashboard is static

`Dashboard.razor` contains hard-coded statistics, tasks, referrals, visits, and announcements.

## 83.2 Patient workflow is static

`PatientWorkflow.razor` contains hard-coded patient and workflow data.

## 83.3 Referral wizard does not submit

The `/referrals` page currently changes wizard steps but does not create a referral through the API.

## 83.4 Several patient tabs are UI only

Insurance, Compliance, Documents, Communication, Timeline, and Audit Log are not currently wired to corresponding complete API features.

## 83.5 GET patients lacks a permission policy

`PatientsController.GetPatients()` is protected by controller-level `[Authorize]`, but it does not specify:

```text
patients.view
```

## 83.6 Web pages mostly use authentication-only CcapAuthorize

Current pages generally use:

```razor
@attribute [CcapAuthorize]
```

rather than:

```razor
@attribute [CcapAuthorize(Policy = "...")]
```

## 83.7 Call-note actor identity is supplied by the command

The backend currently accepts `RecordedByUserId` from the request rather than deriving it from the JWT.

## 83.8 Dashboard profile is dynamic, but dashboard content is not

The Topbar uses JWT claims for the current user, while Dashboard sample task data is still hard-coded.

---

# 84. Current Web Program Configuration

The Web registers:

```text
Razor Components
Interactive Server Components
CascadingAuthenticationState
ProtectedLocalStorage
TokenStore
CcapAuthenticationStateProvider
AuthenticationService
CcapApiClient
UserServices
AdminLookupService
RoleService
PatientService
PatientClinicalService
```

The API URL is read from:

```text
Api:BaseUrl
```

The current `Program.cs` registers `CcapApiClient` twice. The duplicate registration should be cleaned up so the service is registered only once.

The current `ApiAuthenticationHandler` class still exists in the project, but the current `Program.cs` does not attach it to the HttpClient pipeline. Bearer-token handling is currently performed directly by `CcapApiClient`.

---

# 85. API Program Configuration

The API registers:

```text
Controllers
Swagger
Application
Infrastructure
JWT Bearer Authentication
CCAP Permission Policies
```

Startup database behavior:

```text
Database:ApplyMigrations
```

If true:

```csharp
context.Database.MigrateAsync()
```

is executed during API startup.

The seeder runs during startup regardless of whether automatic migrations are enabled.

---

# 86. Recommended Production Boundary

The production architecture should remain:

```text
Internet
   ↓
HTTPS
   ↓
CCAP.Web
   ↓
HTTPS
   ↓
CCAP.API
   ↓
JWT validation
   ↓
Permission authorization
   ↓
Application
   ↓
Infrastructure
   ↓
SQL Server
```

The browser must never directly connect to SQL Server.

---

# 87. How to Add a New API Feature

For a new feature, follow the existing architecture.

## Step 1 - Domain

If the feature requires new business data:

```text
CCAP.Domain/Entities/
```

Add the entity and business rules.

## Step 2 - Application abstraction

Add repository interfaces under:

```text
CCAP.Application/Abstractions/Persistence/
```

## Step 3 - Application use case

Add:

```text
Commands
Queries
Handlers
DTOs
```

under:

```text
CCAP.Application/Features/<Feature>/
```

## Step 4 - Infrastructure repository

Implement the repository under:

```text
CCAP.Infrastructure/Persistence/Repositories/
```

## Step 5 - EF configuration

Add the DbSet and entity mapping to:

```text
AppDbContext
```

## Step 6 - Migration

Create an EF migration.

## Step 7 - API controller

Add the HTTP endpoint under:

```text
CCAP.API/Controllers/
```

Apply the correct permission policy.

## Step 8 - Web service

Add a Web service using:

```text
CcapApiClient
```

## Step 9 - Web page/component

Inject the service into the Razor page.

## Step 10 - UI authorization

Add the appropriate:

```text
CcapAuthorize
```

policy if the page itself should require a permission.

---

# 88. Adding a New Permission

A permission must exist in the database and in the API policy configuration.

Example conceptual permission:

```text
compliance.manage
```

The complete flow is:

```text
Permission record
      ↓
RolePermission
      ↓
JWT permission claim
      ↓
API PermissionPolicy
      ↓
[Authorize(Policy = "compliance.manage")]
```

If the Web page also needs route-level enforcement, use:

```razor
@attribute [CcapAuthorize(Policy = "compliance.manage")]
```

---

# 89. Adding a New Database Entity

Follow:

```text
Domain Entity
      ↓
AppDbContext DbSet
      ↓
OnModelCreating configuration
      ↓
Repository interface
      ↓
Repository implementation
      ↓
Application command/query
      ↓
API controller
      ↓
Web service
      ↓
Razor UI
```

Then create and commit an EF migration.

---

# 90. Design Principles Currently Used

## Domain-first business rules

State transitions such as:

```text
CompleteCare
Archive
Task.Start
Task.Complete
Task.Cancel
Referral.Assign
```

are implemented inside Domain entities.

## CQRS

Application commands and queries separate:

```text
writes
```

from:

```text
reads
```

## Repository abstraction

Application does not directly depend on EF Core.

## Unit of Work

Commands save through:

```text
IUnitOfWork
```

## JWT authentication

The API uses bearer tokens rather than server-side login cookies.

## Database-driven permissions

Roles map to permissions through a relational database.

---

# 91. Testing Flow

The repository's intended local test flow is:

```text
Start CCAP.API
       +
Start CCAP.Web
       ↓
/login
       ↓
admin@ccap.local
       ↓
Dashboard
       ↓
/admin/users
       ↓
Create/edit/activate/deactivate users
       ↓
/admin/roles
       ↓
Change role permissions
       ↓
Logout
       ↓
Login again
       ↓
JWT contains new permission claims
```

---

# 92. Authentication Test

Verify:

```text
Login succeeds
```

Then:

```text
Dashboard loads
```

Then refresh:

```text
JWT restored
```

Then open an API-backed page:

```text
Patients
Users
Roles
```

The Network request should contain:

```http
Authorization: Bearer <JWT>
```

---

# 93. Permission Test

Login as a user with limited permissions.

For example:

```text
Clinician
```

The JWT should contain:

```text
patients.view
patients.manage
referrals.view
```

but not:

```text
users.manage
roles.manage
```

API requests requiring missing permissions should return:

```text
403 Forbidden
```

---

# 94. Database Error: Invalid Object Name 'Permissions'

If the application reports:

```text
Invalid object name 'Permissions'
```

the running database does not contain the schema expected by the current code.

The application expects:

```text
Permissions
Roles
RolePermissions
```

and other current tables.

Resolve this by applying the correct EF migration or appropriate database bootstrap/upgrade script to the **same database referenced by the API connection string**.

Do not solve this by adding a fake in-memory list of permissions to the Web UI.

---

# 95. Authentication Error: IAuthenticationService

The current architecture intentionally does not use:

```text
HttpContext.SignInAsync()
HttpContext.ChallengeAsync()
```

for the Blazor Web login flow.

The API uses ASP.NET Core JWT authentication.

The Web uses:

```text
AuthenticationStateProvider
```

If a Web page accidentally invokes server HTTP authentication APIs, it can produce:

```text
Unable to find the required 'IAuthenticationService'
```

The correct boundary is:

```text
CCAP.Web
    AuthenticationStateProvider

CCAP.API
    AddAuthentication()
    AddJwtBearer()
```

---

# 96. Authentication Error: JavaScript Interop During Static Rendering

The current TokenStore deliberately separates:

```text
GetAsync()
```

from:

```text
LoadPersistedAsync()
```

because browser storage cannot be accessed before interactive rendering.

Correct:

```text
OnAfterRenderAsync
      ↓
LoadPersistedAsync
      ↓
ProtectedLocalStorage
```

Incorrect:

```text
GetAuthenticationStateAsync
      ↓
ProtectedLocalStorage
```

during static rendering.

---

# 97. Current Route Map

```text
/
/dashboard
/login
/patients
/admin/users
/admin/roles
/referrals
/tracker/patient
/counter
/not-found
/Error
```

The root route:

```text
/
```

requires `CcapAuthorize` and redirects to:

```text
/dashboard
```

---

# 98. Navigation Model

The authenticated layout contains:

```text
MainLayout
 ├── NavMenu
 ├── Topbar
 │    ├── Notifications
 │    └── User Profile
 └── Page Body
```

The login page uses:

```text
LoginLayout
```

and therefore does not display the authenticated navigation.

---

# 99. Current Static vs Dynamic Data Summary

## Dynamic/API-backed

```text
Login
Users
Roles
Permissions
Disciplines
Patients
Service Types
```

## Backend implemented but Web not fully connected

```text
Call Notes
Complete Care
Archive Patient
Referral domain
Visits
Tasks
Assessments
Compliance
Activities
Patient Service Orders
```

## Current UI prototypes/static

```text
Dashboard
Referral Intake
Patient Workflow
Insurance
Documents
Communication UI
Timeline UI
Audit Log UI
```

---

# 100. Development Roadmap Based on Current Code

The next development phases should connect the existing UI to the existing backend foundations.

## Phase 1 - Secure current API

- Add `patients.view` to `GET /api/patients`.
- Derive actor/user IDs from JWT claims rather than client-provided IDs.
- Apply specific Web `CcapAuthorize` policies to pages where appropriate.
- Remove duplicate `CcapApiClient` registration.
- Remove or formally deprecate the unused `ApiAuthenticationHandler`.

## Phase 2 - Complete patient workflow API

Implement API/Application support for:

```text
Patient detail
Call notes
Assessments
Compliance
Visits
Tasks
Activities
Service orders
```

## Phase 3 - Connect Patient Workflow UI

Replace static:

```text
John Smith
workflow stages
activities
next actions
summary
```

with API-backed data.

## Phase 4 - Complete Referral Intake

Add:

```text
ReferralController
Referral commands
Referral DTOs
Referral Web service
```

and connect the five-step wizard to the backend.

## Phase 5 - Dashboard API

Create:

```text
Dashboard query
Dashboard DTO
Dashboard controller
Dashboard Web service
```

Replace hard-coded dashboard data.

## Phase 6 - Documents and Insurance

Introduce actual domain/database/API models if these are required by the business workflow.

## Phase 7 - Audit and Timeline

Use `Activity` and/or introduce a dedicated audit model to provide a reliable history of patient changes and user actions.

---

# 101. Important Architectural Rule

Do not solve missing functionality by putting database logic into Razor components.

Avoid:

```text
Razor
   ↓
DbContext
```

The correct path is:

```text
Razor
   ↓
Web Service
   ↓
API
   ↓
Application
   ↓
Repository
   ↓
Infrastructure
   ↓
Database
```

---

# 102. Important Security Rule

The Web UI is not the final security authority.

For example, hiding:

```text
Delete User
```

because the current user lacks a permission is useful for UX.

But the API must still enforce:

```text
users.manage
```

on:

```http
DELETE /api/users/{id}
```

A malicious client can call the API without using the CCAP Web UI.

Therefore:

```text
Web authorization = UI protection
API authorization = security boundary
```

---

# 103. Important Data Integrity Rule

Business state transitions belong in the Domain.

For example:

```text
Active → Completed → Archived
```

should not be implemented only in a Razor button handler.

The current `Patient` domain methods already enforce:

```text
CompleteCare only from Active
Archive only from Completed
```

This protects the business rule regardless of which UI calls the API.

---

# 104. Current System Boundary

The current codebase is best described as:

```text
                ┌──────────────────────────────┐
                │          CCAP.Web            │
                │                              │
                │ Login                        │
                │ Users                        │
                │ Roles                        │
                │ Patients                     │
                │ Dashboard prototype          │
                │ Referral prototype            │
                │ Patient Workflow prototype   │
                └──────────────┬───────────────┘
                               │
                         JWT Bearer
                               │
                ┌──────────────▼───────────────┐
                │          CCAP.API             │
                │                              │
                │ Authentication               │
                │ Permission authorization     │
                │ Users                        │
                │ Roles/Permissions            │
                │ Patients                     │
                └──────────────┬───────────────┘
                               │
                         MediatR/Application
                               │
                ┌──────────────▼───────────────┐
                │      CCAP.Application         │
                │                              │
                │ Commands                     │
                │ Queries                      │
                │ DTOs                         │
                │ Repository abstractions      │
                └──────────────┬───────────────┘
                               │
                ┌──────────────▼───────────────┐
                │     CCAP.Infrastructure       │
                │                              │
                │ EF Core                      │
                │ SQL Server                   │
                │ Repositories                 │
                │ JWT implementation           │
                │ Password hashing              │
                │ Seeder                       │
                └──────────────┬───────────────┘
                               │
                ┌──────────────▼───────────────┐
                │          SQL Server            │
                │                              │
                │ Users/Roles/Permissions      │
                │ Patients/Referrals           │
                │ Clinical records              │
                └──────────────────────────────┘
```

---

# 105. Final Developer Reference

When working on CCAP, use this mental model:

```text
DOMAIN
"What is the business rule?"

APPLICATION
"What operation does the system perform?"

INFRASTRUCTURE
"How is the data/security implementation performed?"

API
"How is the operation exposed and secured over HTTP?"

WEB
"How does the user interact with the operation?"
```

For example, completing patient care:

```text
WEB
User clicks Complete Care
        ↓
WEB SERVICE
POST /api/patients/{id}/complete-care
        ↓
API
Authorize patients.manage
        ↓
APPLICATION
CompleteCareCommandHandler
        ↓
INFRASTRUCTURE
PatientRepository
        ↓
DOMAIN
Patient.CompleteCare()
        ↓
UNIT OF WORK
SaveChangesAsync()
        ↓
SQL SERVER
Patient.Status = Completed
```

This is the core development pattern that should be followed for future CCAP features.

---
