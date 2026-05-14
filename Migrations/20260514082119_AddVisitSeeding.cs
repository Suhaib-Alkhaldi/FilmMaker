using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestDetails",
                table: "LocationBookingRequests");

            migrationBuilder.DropColumn(
                name: "RequestedAtUtc",
                table: "LocationBookingRequests");

            migrationBuilder.RenameColumn(
                name: "VisitDateTime",
                table: "LocationVisitRequests",
                newName: "VisitDate");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "LocationVisitRequests",
                newName: "Message");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "LocationBookingRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "LocationBookingRequests");

            migrationBuilder.RenameColumn(
                name: "VisitDate",
                table: "LocationVisitRequests",
                newName: "VisitDateTime");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "LocationVisitRequests",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "RequestDetails",
                table: "LocationBookingRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAtUtc",
                table: "LocationBookingRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
