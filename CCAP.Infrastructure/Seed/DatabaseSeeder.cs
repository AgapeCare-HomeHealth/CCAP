using CCAP.Application.Abstractions.Identity;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {

        var permissions = new (string Code, string Name, string Module)[]
        {
            ("users.view", "View Users", "Users"),
            ("users.manage", "Manage Users", "Users"),
            ("roles.view", "View Roles", "Roles"),
            ("roles.manage", "Manage Roles & Permissions", "Roles"),
            ("patients.view", "View Patients", "Patients"),
            ("patients.manage", "Manage Patients", "Patients"),
            ("referrals.view", "View Referrals", "Referrals"),
            ("referrals.manage", "Manage Referrals", "Referrals"),
            ("fax.view", "View Referring Fax Information", "Patient Profile"),
            ("fax.manage", "Manage Referring Fax Information", "Patient Profile"),
            ("notifications.view", "View Patient Notifications", "Patient Profile"),
            ("notifications.manage", "Manage Patient Notifications", "Patient Profile"),
            ("notes.view", "View Patient Notes", "Patient Profile"),
            ("notes.manage", "Manage Patient Notes", "Patient Profile"),
            ("labs.view", "View Lab Orders", "Clinical"),
            ("labs.manage", "Manage Lab Orders", "Clinical"),
            ("supplies.view", "View Wound Supplies", "Clinical"),
            ("supplies.manage", "Manage Wound Supplies", "Clinical"),
            ("foley.view", "View Foley Changes", "Clinical"),
            ("foley.manage", "Manage Foley Changes", "Clinical"),
            ("orders.view", "View Order Alerts", "Orders"),
            ("orders.manage", "Manage Order Alerts", "Orders")
        };

        var permissionEntities = new List<Permission>();
        foreach (var item in permissions)
        {
            var existing = await context.Permissions
                .FirstOrDefaultAsync(x => x.PermissionCode == item.Code, cancellationToken);

            if (existing is null)
            {
                existing = new Permission(item.Code, item.Name, item.Module);
                context.Permissions.Add(existing);
            }

            permissionEntities.Add(existing);
        }

        var roleDefinitions = new[]
        {
            ("Administrator", "Full system administration"),
            ("Care Coordinator", "Coordinates referrals and patient care"),
            ("Clinician", "Provides and documents patient care"),
            ("Scheduler", "Manages scheduling and visits")
        };

        var roles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in roleDefinitions)
        {
            var role = await context.Roles
                .FirstOrDefaultAsync(x => x.RoleName == definition.Item1, cancellationToken);

            if (role is null)
            {
                role = new Role(definition.Item1, definition.Item2);
                context.Roles.Add(role);
            }

            roles[role.RoleName] = role;
        }

        await context.SaveChangesAsync(cancellationToken);

        // Administrator receives all permissions.
        var adminRole = roles["Administrator"];
        var existingAdminPermissionIds = await context.RolePermissions
            .Where(x => x.RoleId == adminRole.RoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissionEntities)
        {
            if (!existingAdminPermissionIds.Contains(permission.PermissionId))
                context.RolePermissions.Add(new RolePermission(adminRole.RoleId, permission.PermissionId));
        }

        // Other roles receive only the permissions needed for their initial workflow.
        var rolePermissionCodes = new Dictionary<string, string[]>
        {
            ["Care Coordinator"] =
            ["users.view", "roles.view", "patients.view", "patients.manage", "referrals.view", "referrals.manage",
             "fax.view", "fax.manage", "notifications.view", "notifications.manage", "notes.view", "notes.manage",
             "labs.view", "labs.manage", "supplies.view", "supplies.manage", "foley.view", "foley.manage",
             "orders.view", "orders.manage"],
            ["Clinician"] =
            ["patients.view", "patients.manage", "referrals.view", "fax.view", "fax.manage", "notifications.view", "notifications.manage",
             "notes.view", "notes.manage", "labs.view", "labs.manage", "supplies.view", "supplies.manage",
             "foley.view", "foley.manage", "orders.view", "orders.manage"],
            ["Scheduler"] =
            ["patients.view", "referrals.view", "referrals.manage"]
        };

        foreach (var pair in rolePermissionCodes)
        {
            var role = roles[pair.Key];

            var existingIds = await context.RolePermissions
                .Where(x => x.RoleId == role.RoleId)
                .Select(x => x.PermissionId)
                .ToListAsync(cancellationToken);

            var selected = permissionEntities
                .Where(p => pair.Value.Contains(
                    p.PermissionCode,
                    StringComparer.OrdinalIgnoreCase));

            foreach (var permission in selected)
            {
                if (!existingIds.Contains(permission.PermissionId))
                    context.RolePermissions.Add(new RolePermission(role.RoleId, permission.PermissionId));
            }
        }

        var disciplines = new[]
        {
            ("RN", "Registered Nurse"),
            ("LVN", "Licensed Vocational Nurse"),
            ("PT", "Physical Therapy"),
            ("OT", "Occupational Therapy"),
            ("ST", "Speech Therapy"),
            ("HHA", "Home Health Aide")
        };

        foreach (var item in disciplines)
        {
            if (!await context.Disciplines.AnyAsync(
                x => x.Code == item.Item1,
                cancellationToken))
            {
                context.Disciplines.Add(
                    new Discipline(item.Item1, item.Item2));
            }
        }

        // Seed active clinical service types used by the workflow UI.
        var serviceEntities = new[]
        {
            ("SN", "Skilled Nursing", "bi bi-heart-pulse", "text-danger"),
            ("PT", "Physical Therapy", "bi bi-person-walking", "text-primary"),
            ("OT", "Occupational Therapy", "bi bi-universal-access", "text-success"),
            ("ST", "Speech Therapy", "bi bi-chat-dots", "text-info"),
            ("HHA", "Home Health Aide", "bi bi-person-heart", "text-warning")
        };

        // Remove any accidental empty additions from the first pass and use
        // reflection-free factory support added to ServiceType.
        context.ServiceTypes.RemoveRange(
            context.ServiceTypes.Where(x => string.IsNullOrWhiteSpace(x.Code)));

        foreach (var item in serviceEntities)
        {
            if (!await context.ServiceTypes.AnyAsync(
                x => x.Code == item.Item1,
                cancellationToken))
            {
                context.ServiceTypes.Add(
                    ServiceType.Create(item.Item1, item.Item2, item.Item3, item.Item4));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var adminEmail = "admin@ccap.local";
        var adminUser = await context.ApplicationUsers
            .FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser(
                "CCAP-ADMIN",
                "CCAP",
                "Administrator",
                adminEmail,
                string.Empty,
                adminRole.RoleId,
                null);

            adminUser.SetPasswordHash(
                passwordHasher.HashPassword(adminUser, "Admin123!"));

            context.ApplicationUsers.Add(adminUser);
        }
        else if (!adminUser.IsActive)
        {
            adminUser.Activate();
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
