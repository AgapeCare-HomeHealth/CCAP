using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralDrraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferralDrafts",
                columns: table => new
                {
                    ReferralDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralDrafts", x => x.ReferralDraftId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_CreatedByUserId",
                table: "ReferralDrafts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_Status",
                table: "ReferralDrafts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralDrafts_UpdatedAt",
                table: "ReferralDrafts",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralDrafts");
        }
    }
}
