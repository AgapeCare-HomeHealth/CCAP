using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261117161000_AddCalendarImportCareCoordinatorAssignment")]
public sealed class AddCalendarImportCareCoordinatorAssignment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_CalendarImportBatches_CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches",
            column: "CareCoordinatorId");

        migrationBuilder.AddForeignKey(
            name: "FK_CalendarImportBatches_ApplicationUsers_CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches",
            column: "CareCoordinatorId",
            principalSchema: "dbo",
            principalTable: "ApplicationUsers",
            principalColumn: "UserId",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CalendarImportBatches_ApplicationUsers_CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches");

        migrationBuilder.DropIndex(
            name: "IX_CalendarImportBatches_CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches");

        migrationBuilder.DropColumn(
            name: "CareCoordinatorId",
            schema: "dbo",
            table: "CalendarImportBatches");
    }
}
