using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class AddPatientCareLogStatusAndOrderDate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            schema: "dbo",
            table: "PatientCareLogs",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "OrderDate",
            schema: "dbo",
            table: "PatientServiceOrders",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            schema: "dbo",
            table: "PatientCareLogs");

        migrationBuilder.DropColumn(
            name: "OrderDate",
            schema: "dbo",
            table: "PatientServiceOrders");
    }
}
