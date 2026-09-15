using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupOptionsAndWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupOptions",
                columns: table => new
                {
                    LookupOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupOptions", x => x.LookupOptionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupOptions_LookupType_Code",
                table: "LookupOptions",
                columns: new[] { "LookupType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupOptions_LookupType_IsActive_SortOrder",
                table: "LookupOptions",
                columns: new[] { "LookupType", "IsActive", "SortOrder" });

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreAuthDueDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfVisits",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseMixType",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DmeMedSupplyNotes",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocFeedbackFromPatient",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TifDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RocDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RecertDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PcpPtNotified",
                table: "Patients",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DischargeDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargeFeedback",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferDestination",
                table: "Patients",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TransferDate",
                table: "Patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReason",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PreAuthDueDate", table: "Patients");
            migrationBuilder.DropColumn(name: "NumberOfVisits", table: "Patients");
            migrationBuilder.DropColumn(name: "CaseMixType", table: "Patients");
            migrationBuilder.DropColumn(name: "DmeMedSupplyNotes", table: "Patients");
            migrationBuilder.DropColumn(name: "SocFeedbackFromPatient", table: "Patients");
            migrationBuilder.DropColumn(name: "TifDate", table: "Patients");
            migrationBuilder.DropColumn(name: "RocDate", table: "Patients");
            migrationBuilder.DropColumn(name: "RecertDate", table: "Patients");
            migrationBuilder.DropColumn(name: "PcpPtNotified", table: "Patients");
            migrationBuilder.DropColumn(name: "DischargeDate", table: "Patients");
            migrationBuilder.DropColumn(name: "DischargeFeedback", table: "Patients");
            migrationBuilder.DropColumn(name: "TransferDestination", table: "Patients");
            migrationBuilder.DropColumn(name: "TransferDate", table: "Patients");
            migrationBuilder.DropColumn(name: "TransferReason", table: "Patients");
            migrationBuilder.DropTable(name: "LookupOptions");
        }
    }
}
