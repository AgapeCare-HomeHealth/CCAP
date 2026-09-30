using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations;

public partial class RemovePatientComplianceDocuments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PatientComplianceDocuments");

        migrationBuilder.Sql("""
            UPDATE ComplianceRecords
            SET Notes = 'Review the insurance document in the EMR and confirm the insurance name, member ID, and authorization details match. Mark this item complete only after everything has been verified.'
            WHERE RequirementCode = 'INSURANCE_VERIFICATION';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PatientComplianceDocuments",
            columns: table => new
            {
                PatientComplianceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RequirementCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientComplianceDocuments", x => x.PatientComplianceDocumentId);
                table.ForeignKey(
                    name: "FK_PatientComplianceDocuments_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "PatientId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PatientComplianceDocuments_PatientId_RequirementCode_UploadedAt",
            table: "PatientComplianceDocuments",
            columns: new[] { "PatientId", "RequirementCode", "UploadedAt" });
    }
}
