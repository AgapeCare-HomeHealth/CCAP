using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class AddCalendarScheduleFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "RecurrenceGroupId", schema: "dbo", table: "Visits", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<int>(
            name: "VisitsEachDay", schema: "dbo", table: "Visits", type: "int", nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<string>(
            name: "Skills", schema: "dbo", table: "Visits", type: "nvarchar(300)", maxLength: 300, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "TreatmentAuthorizationCode", schema: "dbo", table: "Visits", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<bool>(
            name: "FlexibleVisit", schema: "dbo", table: "Visits", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(
            name: "OutsourcedVisit", schema: "dbo", table: "Visits", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(
            name: "AttachmentsNeeded", schema: "dbo", table: "Visits", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "BillingRateCode", schema: "dbo", table: "Visits", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "BillingRevenueCode", schema: "dbo", table: "Visits", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<decimal>(
            name: "BillingAmount", schema: "dbo", table: "Visits", type: "decimal(18,2)", precision: 18, scale: 2, nullable: true);
        migrationBuilder.AddColumn<decimal>(
            name: "BillingOvertimeAmount", schema: "dbo", table: "Visits", type: "decimal(18,2)", precision: 18, scale: 2, nullable: true);
        migrationBuilder.CreateIndex(
            name: "IX_Visits_RecurrenceGroupId", schema: "dbo", table: "Visits", column: "RecurrenceGroupId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Visits_RecurrenceGroupId", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "RecurrenceGroupId", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "VisitsEachDay", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "Skills", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "TreatmentAuthorizationCode", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "FlexibleVisit", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "OutsourcedVisit", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "AttachmentsNeeded", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "BillingRateCode", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "BillingRevenueCode", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "BillingAmount", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "BillingOvertimeAmount", schema: "dbo", table: "Visits");
    }
}
