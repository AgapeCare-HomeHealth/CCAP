using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CCAP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261117144500_AddCalendarImport")]
public sealed class AddCalendarImport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CalendarImportBatches",
            schema: "dbo",
            columns: table => new
            {
                CalendarImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                TotalRows = table.Column<int>(type: "int", nullable: false),
                InsertedRows = table.Column<int>(type: "int", nullable: false),
                DuplicateRows = table.Column<int>(type: "int", nullable: false),
                FailedRows = table.Column<int>(type: "int", nullable: false),
                WarningRows = table.Column<int>(type: "int", nullable: false),
                UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarImportBatches", x => x.CalendarImportBatchId);
            });

        migrationBuilder.CreateTable(
            name: "CalendarImportRows",
            schema: "dbo",
            columns: table => new
            {
                CalendarImportRowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CalendarImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RowNumber = table.Column<int>(type: "int", nullable: false),
                SheetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                RawDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                HasWarning = table.Column<bool>(type: "bit", nullable: false),
                ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CalendarImportRows", x => x.CalendarImportRowId);
                table.ForeignKey(
                    name: "FK_CalendarImportRows_CalendarImportBatches_CalendarImportBatchId",
                    column: x => x.CalendarImportBatchId,
                    principalTable: "CalendarImportBatches",
                    principalColumn: "CalendarImportBatchId",
                    principalSchema: "dbo",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_CalendarImportBatches_Status", "CalendarImportBatches", "Status", schema: "dbo");
        migrationBuilder.CreateIndex("IX_CalendarImportBatches_UploadedAt", "CalendarImportBatches", "UploadedAt", schema: "dbo");
        migrationBuilder.CreateIndex("IX_CalendarImportRows_CalendarImportBatchId_SheetName_RowNumber", "CalendarImportRows", new[] { "CalendarImportBatchId", "SheetName", "RowNumber" }, unique: true, schema: "dbo");
        migrationBuilder.CreateIndex("IX_CalendarImportRows_CalendarImportBatchId_Status", "CalendarImportRows", new[] { "CalendarImportBatchId", "Status" }, schema: "dbo");
        migrationBuilder.CreateIndex("IX_Visits_ScheduledDate", "Visits", "ScheduledDate", schema: "dbo");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Visits_ScheduledDate", table: "Visits", schema: "dbo");
        migrationBuilder.DropTable(name: "CalendarImportRows", schema: "dbo");
        migrationBuilder.DropTable(name: "CalendarImportBatches", schema: "dbo");
    }
}
