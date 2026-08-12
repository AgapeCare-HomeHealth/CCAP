# CCAP Local Test + Deployment Guide

## 1. Database

### Existing development database

If the database already exists but reports `Invalid object name 'Permissions'`, run:

`database/CCAP-Admin-Upgrade.sql`

against that exact CCAP database, or create/apply the corresponding EF migration.

### Fresh development database

Run `database/CCAP-FreshDatabase.sql` in SQL Server Management Studio.

The API seeder then inserts the initial roles, permissions, disciplines, service types and development admin.

## 2. EF migrations

The repository contains `AppDbContextFactory` specifically for EF design-time commands.

Package Manager Console:

```powershell
Get-Migration -Project CCAP.Infrastructure -StartupProject CCAP.API
Add-Migration <MigrationName> -Project CCAP.Infrastructure -StartupProject CCAP.API
Update-Database -Project CCAP.Infrastructure -StartupProject CCAP.API
Script-Migration -Idempotent -Project CCAP.Infrastructure -StartupProject CCAP.API
```

Commit generated `Persistence/Migrations` files to Git. Do not ignore them.

For production, prefer applying an idempotent migration script as a deployment step. Set `Database:ApplyMigrations=true` only when you intentionally want the API to apply committed migrations on startup.

## 3. Start both projects

Visual Studio -> Multiple startup projects:

- `CCAP.API`: Start
- `CCAP.Web`: Start

The supplied development URLs use API `https://localhost:7218` and Web `http://localhost:5202` (or `https://localhost:7202`). The API and Web no longer share the same HTTPS port.

## 4. Authentication

The first protected route redirects to `/login`.

Development account:

- Email: `admin@ccap.local`
- Password: `Admin123!`

After login, the JWT is stored in protected browser storage and is sent to the API as a Bearer token.

## 5. Dynamic profile

The top-right profile is read from the authenticated JWT claims:

- Name
- Role
- Permissions

It is no longer hardcoded to Jennifer LVN / Care Coordinator.

Logout removes the token and returns to `/login`.

## 6. Production

Do not deploy the development connection string or JWT key. Supply them through server configuration/secrets. Do not use the development administrator password in production.
