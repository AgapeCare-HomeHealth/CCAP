using Microsoft.EntityFrameworkCore.Migrations; 
using Microsoft.EntityFrameworkCore.Infrastructure; 
using CCAP.Infrastructure.Persistence;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))] 
[Migration("20261103140000_AddReferralTrackerFields")] 
public partial class AddReferralTrackerFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PostedOn", schema: "dbo", table: "Referrals",
            type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "TrackerCoordinatorName", schema: "dbo", table: "Referrals",
            type: "nvarchar(150)", maxLength: 150, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "TrackerStatus", schema: "dbo", table: "Referrals",
            type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<DateTime>(
            name: "EstimatedDischargeDate", schema: "dbo", table: "Referrals",
            type: "date", nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "FirstMarketingCall", schema: "dbo", table: "Referrals",
            type: "nvarchar(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "SecondCoordinatorCall", schema: "dbo", table: "Referrals",
            type: "nvarchar(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "ThirdCoordinatorCall", schema: "dbo", table: "Referrals",
            type: "nvarchar(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "Resolution", schema: "dbo", table: "Referrals",
            type: "nvarchar(500)", maxLength: 500, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PostedOn", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "TrackerCoordinatorName", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "TrackerStatus", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "EstimatedDischargeDate", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "FirstMarketingCall", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "SecondCoordinatorCall", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "ThirdCoordinatorCall", schema: "dbo", table: "Referrals");
        migrationBuilder.DropColumn(name: "Resolution", schema: "dbo", table: "Referrals");
    }
}
