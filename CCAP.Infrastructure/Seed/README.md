# Database-driven lookup data

The UI no longer hardcodes business lookup values for roles, disciplines, or
clinical service types. These values must exist in SQL Server.

Required tables:
- Roles
- Permissions
- Disciplines
- ServiceTypes

Insert/update these records through database migrations, an admin feature, or
a controlled deployment seed process. Do not put operational lookup values
inside Razor components.
