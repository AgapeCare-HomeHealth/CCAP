# CCAP deployment-ready authentication baseline

This branch includes the authentication/routing fixes from the local testing cycle while keeping the deployment architecture intact.

## Important fixes included

- `/` is the authenticated application entry route and redirects to `/dashboard` without a full page reload.
- `/login` is the only login route.
- Login uses `LoginLayout`; authenticated pages use `MainLayout`.
- The Web application uses a custom `CcapAuthorizeRouteView` backed by `AuthenticationStateProvider`; it does not depend on `HttpContext.SignInAsync`, `ChallengeAsync`, or server cookie authentication.
- `TokenStore.GetAsync()` never invokes JavaScript interop.
- `ProtectedLocalStorage` is only read by `AuthenticationBootstrap` from `OnAfterRenderAsync`, after the interactive Blazor circuit exists.
- JWT is persisted in protected browser storage and also kept in memory for API calls in the current circuit.
- `ApiAuthenticationHandler` attaches `Authorization: Bearer <JWT>` to API requests.
- API JWT authentication and database-driven permission policies remain the security boundary.
- API controllers continue to enforce permissions with `[Authorize(Policy = "permission.code")]`.
- EF Core design-time `AppDbContextFactory` remains in Infrastructure.
- Fresh SQL Server bootstrap script creates `Permissions`, `Roles`, `RolePermissions`, users, and application tables.

## Local database

For a completely fresh SQL Server database, run:

`database/CCAP-FreshDatabase.sql`

Then start the API. The API seeder is idempotent for the seeded roles, permissions, disciplines, service types, and development administrator.

Development administrator:

- Email: `admin@ccap.local`
- Password: `Admin123!`

Change/remove this seeded account before production use.

## EF Core

The design-time factory is under:

`CCAP.Infrastructure/Persistence/AppDbContextFactory.cs`

Use Package Manager Console with `CCAP.Infrastructure` as the target project when creating migrations. The startup project must reference `Microsoft.EntityFrameworkCore.Design`; this solution includes that package in `CCAP.API` and `CCAP.Infrastructure`.

Example:

`Add-Migration InitialCreate -Project CCAP.Infrastructure -StartupProject CCAP.API`

`Update-Database -Project CCAP.Infrastructure -StartupProject CCAP.API`

For production, review the generated SQL/migration plan and run it as a deployment database step rather than enabling automatic migrations blindly.

## Production configuration

Do not commit production secrets. Set the following through the server/environment secret mechanism:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpireMinutes`
- `Api__BaseUrl`

`appsettings.Production.example.json` files are examples only.

## Authentication flow

Browser -> CCAP.Web login -> CCAP.API -> SQL Server user/role/permissions -> JWT -> protected browser storage -> AuthenticationStateProvider -> API bearer token.

The API remains the final authorization boundary. Hiding a Web button based on a permission is only a UI convenience; the API must continue to reject unauthorized commands with `403 Forbidden`.
