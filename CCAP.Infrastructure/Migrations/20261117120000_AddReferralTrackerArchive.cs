using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261117120000_AddReferralTrackerArchive")]
public sealed class AddReferralTrackerArchive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "ArchivedAt",
            schema: "dbo",
            table: "ReferralTrackerEntries",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ArchivedByUserId",
            schema: "dbo",
            table: "ReferralTrackerEntries",
            type: "uniqueidentifier",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ArchivedAt",
            schema: "dbo",
            table: "ReferralTrackerEntries");

        migrationBuilder.DropColumn(
            name: "ArchivedByUserId",
            schema: "dbo",
            table: "ReferralTrackerEntries");
    }
}
