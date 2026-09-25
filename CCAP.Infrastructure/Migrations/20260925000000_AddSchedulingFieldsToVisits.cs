using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class AddSchedulingFieldsToVisits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CallNotes",
            table: "Visits",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ConfirmationStatus",
            table: "Visits",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NotesFlag",
            table: "Visits",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TimeBlock",
            table: "Visits",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CallNotes", table: "Visits");
        migrationBuilder.DropColumn(name: "ConfirmationStatus", table: "Visits");
        migrationBuilder.DropColumn(name: "NotesFlag", table: "Visits");
        migrationBuilder.DropColumn(name: "TimeBlock", table: "Visits");
    }
}
