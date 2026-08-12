# CCAP Final Architecture

## Clean Architecture

### Domain/Core
Entities, enums and business rules. No EF Core or ASP.NET dependencies.

### Application
CQRS commands/queries, DTOs, repository/security abstractions and use cases.

### Infrastructure
EF Core/SQL Server, repositories, JWT implementation, password hashing and persistence.
`AppDbContextFactory` is an EF Core design-time adapter and lives beside `AppDbContext`.

### Presentation
`CCAP.API` owns HTTP concerns, JWT authentication configuration and ASP.NET Core authorization policies.
`CCAP.Web` is the Blazor presentation layer.

## Authentication

CCAP.Web -> `/api/auth/login` -> CQRS LoginCommand -> Infrastructure user/password verification -> JWT -> protected browser storage -> Bearer API calls.

Blazor uses a custom AuthenticationStateProvider to build the current user from the JWT claims. The topbar/profile therefore reflects the actual logged-in user.

## Authorization

JWT permission claims are mapped by `CCAP.API` to ASP.NET Core policies. Infrastructure does not reference `Microsoft.AspNetCore.Authorization`.

## Database

EF Core migrations are the source of truth for schema evolution. Migrations should be committed to Git. The design-time factory is only for EF tooling; it is not required by the running API.
