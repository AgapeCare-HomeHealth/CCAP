using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261104000000_AddPatientCoordinatorAssignments")]
public sealed class AddPatientCoordinatorAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "SecondaryCoordinatorId",
            schema: "dbo",
            table: "Patients",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "BackupCoordinatorId",
            schema: "dbo",
            table: "Patients",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Patients_SecondaryCoordinatorId",
            schema: "dbo",
            table: "Patients",
            column: "SecondaryCoordinatorId");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_BackupCoordinatorId",
            schema: "dbo",
            table: "Patients",
            column: "BackupCoordinatorId");

        migrationBuilder.AddForeignKey(
            name: "FK_Patients_ApplicationUsers_SecondaryCoordinatorId",
            schema: "dbo",
            table: "Patients",
            column: "SecondaryCoordinatorId",
            principalSchema: "dbo",
            principalTable: "ApplicationUsers",
            principalColumn: "UserId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Patients_ApplicationUsers_BackupCoordinatorId",
            schema: "dbo",
            table: "Patients",
            column: "BackupCoordinatorId",
            principalSchema: "dbo",
            principalTable: "ApplicationUsers",
            principalColumn: "UserId",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Patients_ApplicationUsers_SecondaryCoordinatorId", "Patients", schema: "dbo");
        migrationBuilder.DropForeignKey("FK_Patients_ApplicationUsers_BackupCoordinatorId", "Patients", schema: "dbo");
        migrationBuilder.DropIndex("IX_Patients_SecondaryCoordinatorId", "Patients", schema: "dbo");
        migrationBuilder.DropIndex("IX_Patients_BackupCoordinatorId", "Patients", schema: "dbo");
        migrationBuilder.DropColumn("SecondaryCoordinatorId", "Patients", schema: "dbo");
        migrationBuilder.DropColumn("BackupCoordinatorId", "Patients", schema: "dbo");
    }
}
