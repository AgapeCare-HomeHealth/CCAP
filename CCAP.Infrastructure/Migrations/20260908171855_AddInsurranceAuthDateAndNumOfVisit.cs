using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurranceAuthDateAndNumOfVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedVisits",
                table: "Referrals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AuthorizationDate",
                table: "Referrals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedVisits",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AuthorizationDate",
                table: "Patients",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedVisits",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "AuthorizationDate",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ApprovedVisits",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AuthorizationDate",
                table: "Patients");
        }
    }
}
