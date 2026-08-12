# CCAP Database Setup

## Existing CCAP database

1. Back up the database.
2. Run the EF Core migration that matches the code version.
3. If this ZIP is being applied before migrations are generated, use `CCAP-Admin-Upgrade.sql` for the new admin/security tables.
4. Start `CCAP.API`; the seeder inserts/updates initial roles, permissions, disciplines, service types and the development administrator.

Do not use `EnsureCreated()` for an existing database. Schema changes should be managed by EF Core migrations.

## Fresh database

Use `CCAP-FreshDatabase.sql` on SQL Server to create the database and current CCAP schema, then point `CCAP.API` at it. The application seeder will populate initial data.

For a production deployment, generate an idempotent migration script from the committed EF migrations and execute it as a deployment step.

## EF Package Manager Console

Default project: `CCAP.Infrastructure`
Startup project: `CCAP.API`

```powershell
Get-Migration -Project CCAP.Infrastructure -StartupProject CCAP.API
Add-Migration <MigrationName> -Project CCAP.Infrastructure -StartupProject CCAP.API
Update-Database -Project CCAP.Infrastructure -StartupProject CCAP.API
Script-Migration -Idempotent -Project CCAP.Infrastructure -StartupProject CCAP.API
```

`AppDbContextFactory` exists specifically for design-time EF commands.
