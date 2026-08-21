using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReferralIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Referrals",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AuthorizationRequired",
                table: "Referrals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CaseStatus",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisciplineId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceMemberId",
                table: "Referrals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PhysicianPhone",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryInsurance",
                table: "Referrals",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralNotes",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferringPhysician",
                table: "Referrals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryDiagnosis",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitPriority",
                table: "Referrals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AuthorizationRequired",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Patients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceMemberId",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicianPhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryInsurance",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralNotes",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferringPhysician",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryDiagnosis",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "ReferralDocuments",
                columns: table => new
                {
                    ReferralDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralDocuments", x => x.ReferralDocumentId);
                    table.ForeignKey(
                        name: "FK_ReferralDocuments_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_DisciplineId",
                table: "Referrals",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_LocationId",
                table: "Referrals",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDocuments_ReferralId",
                table: "ReferralDocuments",
                column: "ReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Disciplines_DisciplineId",
                table: "Referrals",
                column: "DisciplineId",
                principalTable: "Disciplines",
                principalColumn: "DisciplineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Locations_LocationId",
                table: "Referrals",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Disciplines_DisciplineId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Locations_LocationId",
                table: "Referrals");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "ReferralDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_DisciplineId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_LocationId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "AuthorizationRequired",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DisciplineId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "InsuranceMemberId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "PhysicianPhone",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "PrimaryInsurance",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferralNotes",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferringPhysician",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "SecondaryDiagnosis",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "VisitPriority",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AuthorizationRequired",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "InsuranceMemberId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PhysicianPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PrimaryInsurance",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReferralNotes",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReferringPhysician",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "SecondaryDiagnosis",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Referrals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
