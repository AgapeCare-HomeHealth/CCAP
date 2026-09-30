using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class AddSchedulingPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @SchedulingViewId uniqueidentifier = NEWID();
            DECLARE @SchedulingManageId uniqueidentifier = NEWID();

            IF NOT EXISTS (
                SELECT 1 FROM dbo.Permissions
                WHERE PermissionCode = N'scheduling.view'
            )
            BEGIN
                INSERT INTO dbo.Permissions
                    (PermissionId, PermissionCode, PermissionName, Module, Description)
                VALUES
                    (@SchedulingViewId, N'scheduling.view', N'View Scheduling Calendar', N'Scheduling',
                     N'View the scheduling calendar and scheduling options.');
            END
            ELSE
            BEGIN
                SELECT @SchedulingViewId = PermissionId
                FROM dbo.Permissions
                WHERE PermissionCode = N'scheduling.view';
            END;

            IF NOT EXISTS (
                SELECT 1 FROM dbo.Permissions
                WHERE PermissionCode = N'scheduling.manage'
            )
            BEGIN
                INSERT INTO dbo.Permissions
                    (PermissionId, PermissionCode, PermissionName, Module, Description)
                VALUES
                    (@SchedulingManageId, N'scheduling.manage', N'Manage Scheduling Calendar', N'Scheduling',
                     N'Add schedules and import schedules using the CC Sched template.');
            END
            ELSE
            BEGIN
                SELECT @SchedulingManageId = PermissionId
                FROM dbo.Permissions
                WHERE PermissionCode = N'scheduling.manage';
            END;

            -- Care Coordinators and Schedulers can use the scheduling workflow.
            INSERT INTO dbo.RolePermissions (RolePermissionId, RoleId, PermissionId)
            SELECT NEWID(), r.RoleId, p.PermissionId
            FROM dbo.Roles r
            CROSS JOIN dbo.Permissions p
            WHERE r.RoleName IN (N'Care Coordinator', N'Scheduler')
              AND p.PermissionCode IN (N'scheduling.view', N'scheduling.manage')
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.RolePermissions rp
                  WHERE rp.RoleId = r.RoleId
                    AND rp.PermissionId = p.PermissionId
              );

            -- Clinicians can view their own calendar but do not receive
            -- scheduling-management access from this migration.
            INSERT INTO dbo.RolePermissions (RolePermissionId, RoleId, PermissionId)
            SELECT NEWID(), r.RoleId, p.PermissionId
            FROM dbo.Roles r
            CROSS JOIN dbo.Permissions p
            WHERE r.RoleName = N'Clinician'
              AND p.PermissionCode = N'scheduling.view'
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.RolePermissions rp
                  WHERE rp.RoleId = r.RoleId
                    AND rp.PermissionId = p.PermissionId
              );

            -- Administrators receive every permission through the normal seed,
            -- but add these two explicitly for existing databases.
            INSERT INTO dbo.RolePermissions (RolePermissionId, RoleId, PermissionId)
            SELECT NEWID(), r.RoleId, p.PermissionId
            FROM dbo.Roles r
            CROSS JOIN dbo.Permissions p
            WHERE r.RoleName = N'Administrator'
              AND p.PermissionCode IN (N'scheduling.view', N'scheduling.manage')
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.RolePermissions rp
                  WHERE rp.RoleId = r.RoleId
                    AND rp.PermissionId = p.PermissionId
              );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE rp
            FROM dbo.RolePermissions rp
            INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
            WHERE p.PermissionCode IN (N'scheduling.view', N'scheduling.manage');

            DELETE FROM dbo.Permissions
            WHERE PermissionCode IN (N'scheduling.view', N'scheduling.manage');
            """);
    }
}
