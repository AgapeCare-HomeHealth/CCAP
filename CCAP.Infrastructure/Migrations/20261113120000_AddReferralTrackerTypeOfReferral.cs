using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261113120000_AddReferralTrackerTypeOfReferral")]
public partial class AddReferralTrackerTypeOfReferral : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateOnly>(
            name: "DateOfBirth",
            schema: "dbo",
            table: "ReferralTrackerEntries",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateOnly),
            oldType: "date");

        migrationBuilder.AddColumn<string>(
            name: "TypeOfReferral",
            schema: "dbo",
            table: "ReferralTrackerEntries",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE [dbo].[ReferralTrackerEntries] SET [DateOfBirth] = '0001-01-01' WHERE [DateOfBirth] IS NULL;");

        migrationBuilder.AlterColumn<DateOnly>(
            name: "DateOfBirth",
            schema: "dbo",
            table: "ReferralTrackerEntries",
            type: "date",
            nullable: false,
            oldClrType: typeof(DateOnly),
            oldType: "date",
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "TypeOfReferral",
            schema: "dbo",
            table: "ReferralTrackerEntries");
    }
}
