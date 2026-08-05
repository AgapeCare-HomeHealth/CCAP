using CCAP.Data.Entities;
using CCAP.Data.Persistence;
using CCAP.Data.Security;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await SeedRoles(context);

            await SeedPermissions(context);

            await SeedDisciplines(context);

            await SeedAdministrator(context);
        }

        private static async Task SeedRoles(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            context.Roles.AddRange(

                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Administrator"
                },

                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Care Coordinator"
                },

                new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Scheduler"
                }

            );

            await context.SaveChangesAsync();
        }

        private static async Task SeedPermissions(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            context.Permissions.AddRange(

                new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    PermissionCode = "USER_VIEW",
                    PermissionName = "View Users",
                    Module = "Users"
                },

                new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    PermissionCode = "USER_CREATE",
                    PermissionName = "Create Users",
                    Module = "Users"
                },

                new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    PermissionCode = "USER_EDIT",
                    PermissionName = "Edit Users",
                    Module = "Users"
                },

                new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    PermissionCode = "USER_DELETE",
                    PermissionName = "Delete Users",
                    Module = "Users"
                }

            );

            await context.SaveChangesAsync();
        }

        private static async Task SeedDisciplines(AppDbContext context)
        {
            if (await context.Disciplines.AnyAsync())
                return;

            context.Disciplines.AddRange(

                new Discipline
                {
                    DisciplineId = Guid.NewGuid(),
                    Code = "RN",
                    Name = "Registered Nurse"
                },

                new Discipline
                {
                    DisciplineId = Guid.NewGuid(),
                    Code = "LVN",
                    Name = "Licensed Vocational Nurse"
                },

                new Discipline
                {
                    DisciplineId = Guid.NewGuid(),
                    Code = "PT",
                    Name = "Physical Therapy"
                },

                new Discipline
                {
                    DisciplineId = Guid.NewGuid(),
                    Code = "OT",
                    Name = "Occupational Therapy"
                },

                new Discipline
                {
                    DisciplineId = Guid.NewGuid(),
                    Code = "ST",
                    Name = "Speech Therapy"
                }

            );

            await context.SaveChangesAsync();
        }

        private static async Task SeedAdministrator(AppDbContext context)
        {
            if (await context.ApplicationUsers.AnyAsync())
                return;

            var role = await context.Roles
                .FirstAsync(r => r.RoleName == "Administrator");

            var hasher = new PasswordHasherService();

            var admin = new ApplicationUser
            {
                UserId = Guid.NewGuid(),
                RoleId = role.RoleId,
                EmployeeNo = "EMP001",
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@ccap.com",
                IsActive = true
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

            context.ApplicationUsers.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}
