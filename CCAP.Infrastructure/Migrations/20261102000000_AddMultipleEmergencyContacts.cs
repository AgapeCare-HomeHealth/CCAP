using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class AddMultipleEmergencyContacts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactsJson",
            schema: "dbo",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EmergencyContactsJson",
            schema: "dbo",
            table: "Patients");
    }
}
