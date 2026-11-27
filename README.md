# Care Coordinator Automation Platform (CCAP)

CCAP means **Care Coordinator Automation Platform**. It is a web application for coordinating patient referrals, care workflow tasks, compliance tracking, scheduling data, documents, communications, and administrative setup.

This README documents the repository as it is implemented today. Business workflow goals that are not fully represented in the data model are called out as limitations or future work; do not treat a desired workflow as an existing automated rule without checking the implementation.

## Contents

- [What the system does](#what-the-system-does)
- [Solution architecture](#solution-architecture)
- [Workflow and data model](#workflow-and-data-model)
- [Developer setup](#developer-setup)
- [Configuration](#configuration)
- [Database and migrations](#database-and-migrations)
- [Files, imports, and OCR](#files-imports-and-ocr)
- [API and realtime features](#api-and-realtime-features)
- [How to change the system](#how-to-change-the-system)
- [Deployment notes](#deployment-notes)
- [Changelog and GitHub releases](#changelog-and-github-releases)
- [Known gaps and future work](#known-gaps-and-future-work)

## What the system does

The solution has two separately hosted ASP.NET Core applications:

- **CCAP.API** exposes authenticated HTTP endpoints and a SignalR notification hub.
- **CCAP.Web** is a server-interactive Blazor application that calls the API.

The product includes patient and referral tracking, referral intake and drafts, patient workflow/compliance records, care management, scheduling calendar and import, patient/referral imports, document extraction, user and role administration, lookup/service type administration, notifications, announcements, and dashboards.

CCAP is a coordination/workflow tool. It is not the clinical EMR and should not be described as the source of truth for clinical visit documentation unless that integration is explicitly implemented.

## Solution architecture

```text
CCAP.Web (Blazor Server / interactive server components)
    | HTTP + JWT; SignalR notifications
    v
CCAP.API (ASP.NET Core controllers, auth, policies, hub, import workers)
    | MediatR requests and application abstractions
    v
CCAP.Application (use cases, validation, DTOs, repository interfaces)
    | interfaces implemented by
    v
CCAP.Infrastructure (EF Core, SQL Server, repositories, storage, OCR/import)
    |
    v
SQL Server and configured file storage

CCAP.Domain (entities and enums; referenced by Application/Infrastructure)
```

| Project | Responsibility |
| --- | --- |
| `CCAP.Domain` | Persistence-agnostic entities and enums. Keep framework and database concerns out of this project. |
| `CCAP.Application` | MediatR commands/queries and handlers, DTOs, validation, and abstractions used by the use cases. References Domain. |
| `CCAP.Infrastructure` | `AppDbContext`, EF Core mappings/migrations, repository and identity implementations, SQL Server, file storage, referral extraction, and scheduling import. References Application and Domain. |
| `CCAP.API` | HTTP controllers/contracts, JWT authentication, authorization policies, exception-to-HTTP handling, Swagger in Development, SignalR hub, and hosted import services. References Application and Infrastructure. |
| `CCAP.Web` | Blazor pages/components, presentation services, authentication state/token handling, and typed/configured API access. |

Application features are grouped by area under `CCAP.Application/Features` (Patients, Referrals, ReferralDrafts, Scheduling, Users, Admin, Dashboard, Notifications, Announcements, Authentication). UI features are grouped under `CCAP.Web/Features`. Infrastructure implementations live under `CCAP.Infrastructure` by concern.

### Request flow

1. A Blazor page calls a Web feature service.
2. The service sends an HTTP request to the configured API base URL; authenticated calls use the user's JWT bearer token.
3. A controller maps the HTTP contract to a MediatR command or query.
4. An application handler validates/executes the use case through Application abstractions.
5. Infrastructure repositories and `AppDbContext` read or write SQL Server. `IFileStorage` abstracts binary storage.
6. The result is mapped back to an HTTP response and rendered by the Web application.

The repository pattern is used for persistence access. Keep handlers dependent on Application abstractions rather than injecting `AppDbContext` into handlers. API authorization policy definitions belong in `CCAP.API`; business validation belongs in Application/domain code.

## Workflow and data model

The core entities include `Patient`, `Referral`, `ReferralDraft`, `ReferralTrackerEntry`, `ReferralDocument`, `ComplianceRecord`, `Visit`, `PatientTask`, `PatientServiceOrder`, `PatientCareLog`, `PatientAuditLog`, `CallNote`, `Activity`, `ApplicationUser`, `Role`, `Permission`, `LookupOption`, `ServiceType`, `Location`, and `Discipline`. Import batches/rows and notification/announcement entities support their respective features. The current EF model is the definitive list of persisted fields and relationships.

The application presents referral intake, insurance, SOC, compliance, care management, documents, communication, timeline/audit, discharge, and scheduling views. Actual workflow operations are implemented as use cases such as insurance verification, SOC scheduling/completion, compliance completion, task completion, care completion, and patient archiving. See `CCAP.Application/Features/Patients` and the patient controllers for the rules enforced by each action.

### Patient versus care episode

The business concept distinguishes a person from a particular period of care. However, the current model has a `Patient` aggregate and does **not** have a dedicated `PatientEpisode`/`CareEpisode` entity. Do not assume multiple independent episodes are fully modeled or that archiving a patient creates a new episode. Episode-specific fields and records may still be attached to the patient. A robust multi-episode design requires a schema and workflow change, with existing history migrated deliberately.

### Workflow terminology and automation

The intended operational journey may be described as referral/intake, insurance, SOC, admission/ongoing care, outcome/discharge, and archive. Individual screens and completion actions exist, but not every business rule in older planning notes is implemented as an automated gate or timer. In particular, do not infer automatic 48-hour/24-hour/5-day SLA enforcement, automatic 60-day discharge, episode creation, or clinical-system synchronization from the presence of a workflow screen or date field. Confirm requirements with the product owner and inspect the corresponding handler before changing behavior.

The Web project includes a scheduling calendar backed by API visit data. This is a CCAP coordination schedule; external clinical systems may remain the source for clinical scheduling/documentation.

## Developer setup

### Prerequisites

- .NET 10 SDK
- SQL Server instance reachable from the API
- Git and an IDE or editor that supports .NET

Some document/OCR dependencies include native/runtime assets; build and publish on the intended operating system and verify the produced output includes the OCR models.

### Restore, build, and run

From the repository root:

```powershell
dotnet restore CCAP.sln
dotnet build CCAP.sln
dotnet run --project CCAP.API
```

In another terminal:

```powershell
dotnet run --project CCAP.Web
```

The API Swagger UI is enabled only when the API runs with the Development environment. The Web app requires a valid `Api:BaseUrl`. Use the API and Web launch settings/appsettings for local URLs; do not assume ports are the same on every developer machine.

The API startup behavior is configuration controlled: `Database:ApplyMigrations` controls EF migration application, and `Database:Seed` controls reference/user seeding. The API also creates the `dbo.UserNotifications` table if it does not exist. Confirm the target database before enabling startup migrations or seed behavior.

## Configuration

The main configuration sections are:

| Application | Key | Purpose |
| --- | --- | --- |
| API | `ConnectionStrings:DefaultConnection` | SQL Server connection used by EF Core. |
| API | `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireMinutes` | JWT signing and token configuration. A sufficiently long secret key is required. |
| API | `Database:ApplyMigrations` | Apply pending EF migrations during API startup when true. |
| API | `Database:Seed` | Run the database seeder during API startup when true. |
| API | `FileStorage:Provider` | `MetadataOnly`/`None`, `Local`, or `AzureBlob`. |
| API | `FileStorage:LocalRootPath` | Root for local binary storage when Local is selected. |
| API | `AzureBlob:ConnectionString`, `ContainerName`, `RootFolder` | Azure Blob configuration when selected. |
| Web | `Api:BaseUrl` | Base address of the API used by the Blazor app. |
| Web | `MockData:Enabled` | Enables the optional mock-data service registration; check feature usage before relying on it. |

For local development, use .NET user secrets, environment variables, or a local untracked configuration file for credentials. In environment variables, use double underscores, for example `ConnectionStrings__DefaultConnection` and `Jwt__Key`. Never commit real database credentials, JWT signing keys, or storage secrets. If a credential has been committed, rotate it and remove it from source-control history as appropriate; deleting it from the latest file alone does not revoke it.

Configuration is layered by environment. Inspect effective settings for both API and Web before running or deploying. Do not copy production connection strings into development setup instructions.

## Database and migrations

SQL Server is configured through `ConnectionStrings:DefaultConnection`. EF Core schema changes are tracked in `CCAP.Infrastructure/Migrations`; `AppDbContextFactory` supports design-time EF operations. Migrations, not manual schema edits, should be the source of truth for normal schema changes.

Create a migration from the solution root:

```powershell
dotnet ef migrations add DescribeTheChange `
  --project CCAP.Infrastructure `
  --startup-project CCAP.API
```

Apply migrations to the configured database:

```powershell
dotnet ef database update `
  --project CCAP.Infrastructure `
  --startup-project CCAP.API
```

Review generated migration code and the model snapshot before committing. For production, coordinate migration execution with deployment and backup procedures; do not point local tooling at production accidentally. Never reset or drop a database containing data you need.

## Files, imports, and OCR

Referral document content is handled through the Application `IFileStorage` abstraction. Infrastructure supports metadata-only/null storage, local filesystem storage, and Azure Blob storage. Metadata-only mode does not preserve the uploaded binary. Select and configure a durable provider when binary retention is required, and consider retention/access controls for patient documents.

Referral extraction is implemented in `CCAP.Infrastructure/Referrals` and exposed through referral API operations. The API project copies RapidOcrNet PP-OCRv5 model files into build/publish output. Keep those copy/publish rules in sync if the OCR package/model version changes. Patient and calendar imports have API controllers and hosted background services; scheduling import processing is implemented in Infrastructure. Review preview/confirm flows and batch/error records before changing import behavior.

## API and realtime features

Controllers are in `CCAP.API/Controllers`. Current route groups include:

| Route group | Area |
| --- | --- |
| `/api/auth` | Login and authentication-related actions |
| `/api/patients` | Patient lists/details, workflow, care management, audit, and patient actions |
| `/api/referrals` | Referral tracker, intake, drafts, and document extraction |
| `/api/scheduling` | Calendar visits, scheduling actions, and import operations |
| `/api/patient-import` | Patient import preview, confirm, status, logs, and errors |
| `/api/referral-import` | Referral history import and batch history |
| `/api/users`, `/api/admin` | User and administration operations |
| `/api/notifications`, `/api/announcements`, `/api/dashboard` | Supporting application features |

The precise verbs, routes, request contracts, and permission requirements are defined by controller attributes and API contracts; use Swagger in Development or inspect the controller before relying on an endpoint list in external documentation. The realtime notifications hub is `/hubs/notifications` and accepts the JWT access token through SignalR's `access_token` query parameter for that hub.

The API exception handler maps validation failures to 400 and selected argument/not-found/authorization/conflict exceptions to corresponding HTTP responses; unexpected exceptions return a generic 500 message. Avoid returning sensitive exception details to clients.

## How to change the system

When implementing a feature:

1. Find the related domain entity and existing feature use cases. Follow the feature folder and naming conventions already present.
2. Put domain invariants with the domain model where appropriate; add command/query, handler, DTO, and validator in Application.
3. Add/update Application abstractions and implement persistence or external integrations in Infrastructure.
4. Add or update API contracts/controller routes and the authorization policy required by the operation.
5. Update the Web feature service and Razor UI to call the API and represent loading, validation, empty, and error states.
6. If the persisted schema changes, add and review an EF migration and update seed data/configuration where required.
7. Update this README when setup, architecture, configuration, operations, or developer workflows change. Add a changelog entry for user-visible or operationally significant changes.

### CQRS and persistence conventions

- Queries read data and should not change state; commands make changes and should validate input/business rules.
- Keep HTTP types at the API boundary and presentation models in Web. Avoid coupling Domain/Application to Blazor or controller types.
- Use repository interfaces in Application and their Infrastructure implementations. Keep database-specific code in Infrastructure.
- Make multi-record changes transactional where the use case requires it; use the existing unit-of-work/repository conventions.
- Preserve audit/history behavior when changing status, compliance, assignment, or archive actions.
- Use lookup/reference data for configurable lists where the current feature already does so; avoid duplicating magic strings across layers.
- Treat imported and extracted values as untrusted input. Preview/validate before applying irreversible or bulk changes.

### Authorization and identity

Authentication uses JWT bearer tokens. Authorization is permission/policy based; policies are registered in `CCAP.API/Authorization`. When adding a protected endpoint, verify both the endpoint's authorization attribute/policy and the corresponding role/permission seed/configuration. UI hiding is not a security boundary: enforce access in the API.

### Validation and error handling

Use Application validators for command/query input and business preconditions. Keep API error responses consistent with the global exception handler. Do not log access tokens, passwords, connection strings, or unnecessary patient data.

## Deployment notes

Publish each host independently:

```powershell
dotnet publish CCAP.API -c Release
dotnet publish CCAP.Web -c Release
```

Configure production connection strings, JWT signing key/issuer/audience, Web API base URL, storage, and allowed hosts through the hosting environment's secret/configuration mechanism. Ensure SQL Server and storage are reachable from the API. Verify native OCR dependencies and model files in the publish artifact. Use HTTPS.

The API can apply migrations at startup when configured. Choose deliberately whether production migrations run at startup or as a separate deployment step, and ensure only one deployment process applies them at a time. Seed behavior should also be explicitly reviewed for the target environment.

## Changelog and GitHub releases

Changelog history belongs in the repository so it is reviewable alongside code. GitHub Releases are an optional distribution/publishing layer; GitHub does not automatically maintain a complete human-readable changelog just because the repository has a README. Release notes can be drafted from pull requests or generated by tooling, but a checked-in changelog remains useful for users and developers.

Use [`CHANGELOG.md`](CHANGELOG.md) for concise, user-facing changes, grouped under `Unreleased` and then version/date headings when a release is made. Record notable additions, behavior changes, fixes, and migration/configuration requirements. Do not duplicate every commit. Link a GitHub Release to the corresponding version/tag when the team starts publishing releases. GitHub Actions, release templates, or automated changelog generation can be added later if the project adopts a release process; no GitHub-side setup is required to start maintaining the file.

## Known gaps and future work

These are architectural/product areas that should be confirmed against current implementation before planning work:

- A dedicated care episode entity and episode-scoped relationships are not present in the current model.
- The business SLAs (for example 48-hour or 24-hour milestones) should not be assumed to be automatically enforced unless a concrete handler/background process does so.
- Automatic 60-day discharge and independent episode history require explicit domain, persistence, scheduling, and migration work.
- External EMR integration and clinical record synchronization are not implied by the current workflow UI.
- Production deployment/release automation is environment-specific; inspect repository workflows and hosting configuration before assuming a CI/CD pipeline exists.

Keep this section current as gaps are implemented. Do not use it as a substitute for acceptance criteria or a source of clinical policy.

## Quick reference

```powershell
dotnet restore CCAP.sln
dotnet build CCAP.sln
dotnet run --project CCAP.API
dotnet run --project CCAP.Web
```

Key locations:

- Domain model: `CCAP.Domain/Entities`, `CCAP.Domain/Enums`
- Use cases and contracts: `CCAP.Application/Features`
- Persistence and migrations: `CCAP.Infrastructure/Persistence`, `CCAP.Infrastructure/Migrations`
- API routes/auth/startup: `CCAP.API/Controllers`, `CCAP.API/Authorization`, `CCAP.API/Program.cs`
- Web UI: `CCAP.Web/Features`, `CCAP.Web/Components`
- Change history: `CHANGELOG.md`
