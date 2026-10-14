using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CCAP.Infrastructure.Migrations;

[Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(CCAP.Infrastructure.Persistence.AppDbContext))]
[Migration("20261014190000_AddVisitCareCoordinator")]
public partial class AddVisitCareCoordinator : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "CareCoordinatorId", schema: "dbo", table: "Visits", type: "uniqueidentifier", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Visits_CareCoordinatorId", schema: "dbo", table: "Visits", column: "CareCoordinatorId");
        migrationBuilder.AddForeignKey(name: "FK_Visits_ApplicationUsers_CareCoordinatorId", schema: "dbo", table: "Visits", column: "CareCoordinatorId",
            principalSchema: "dbo", principalTable: "ApplicationUsers", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Visits_ApplicationUsers_CareCoordinatorId", schema: "dbo", table: "Visits");
        migrationBuilder.DropIndex(name: "IX_Visits_CareCoordinatorId", schema: "dbo", table: "Visits");
        migrationBuilder.DropColumn(name: "CareCoordinatorId", schema: "dbo", table: "Visits");
    }
}
