using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CCAP.Infrastructure.Persistence;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112190000_AddRecertificationRecords")]
public partial class AddRecertificationRecords : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RecertificationRecordsJson",
            schema: "dbo",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "[]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RecertificationRecordsJson",
            schema: "dbo",
            table: "Patients");
    }
}
