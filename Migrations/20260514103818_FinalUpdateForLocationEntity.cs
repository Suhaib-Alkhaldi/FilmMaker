using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class FinalUpdateForLocationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LocationVisitRequests");

            migrationBuilder.RenameColumn(
                name: "VisitDate",
                table: "LocationVisitRequests",
                newName: "RequestedVisitDateUtc");

            migrationBuilder.RenameColumn(
                name: "RequestedById",
                table: "LocationVisitRequests",
                newName: "VisitStatusId");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "LocationVisitRequests",
                newName: "RequestMessage");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "LocationVisitRequests",
                newName: "LocationOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationVisitRequests_RequestedById",
                table: "LocationVisitRequests",
                newName: "IX_LocationVisitRequests_VisitStatusId");

            migrationBuilder.AlterColumn<int>(
                name: "LocationBookingRequestId",
                table: "LocationVisitRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LocationManagerId",
                table: "LocationVisitRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerResponseMessage",
                table: "LocationVisitRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAtUtc",
                table: "LocationVisitRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RespondedByUserId",
                table: "LocationVisitRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationManagerId",
                table: "LocationVisitRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationOwnerId",
                table: "LocationVisitRequests",
                column: "LocationOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_RespondedByUserId",
                table: "LocationVisitRequests",
                column: "RespondedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_LocationManagerId",
                table: "LocationVisitRequests",
                column: "LocationManagerId",
                principalTable: "LocationManagerProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationOwnerProfiles_LocationOwnerId",
                table: "LocationVisitRequests",
                column: "LocationOwnerId",
                principalTable: "LocationOwnerProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LookupItems_VisitStatusId",
                table: "LocationVisitRequests",
                column: "VisitStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_Users_RespondedByUserId",
                table: "LocationVisitRequests",
                column: "RespondedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_LocationManagerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationOwnerProfiles_LocationOwnerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LookupItems_VisitStatusId",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_Users_RespondedByUserId",
                table: "LocationVisitRequests");

            migrationBuilder.DropIndex(
                name: "IX_LocationVisitRequests_LocationManagerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropIndex(
                name: "IX_LocationVisitRequests_LocationOwnerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropIndex(
                name: "IX_LocationVisitRequests_RespondedByUserId",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "LocationManagerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "OwnerResponseMessage",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "RespondedAtUtc",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "RespondedByUserId",
                table: "LocationVisitRequests");

            migrationBuilder.RenameColumn(
                name: "VisitStatusId",
                table: "LocationVisitRequests",
                newName: "RequestedById");

            migrationBuilder.RenameColumn(
                name: "RequestedVisitDateUtc",
                table: "LocationVisitRequests",
                newName: "VisitDate");

            migrationBuilder.RenameColumn(
                name: "RequestMessage",
                table: "LocationVisitRequests",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "LocationOwnerId",
                table: "LocationVisitRequests",
                newName: "DurationMinutes");

            migrationBuilder.RenameIndex(
                name: "IX_LocationVisitRequests_VisitStatusId",
                table: "LocationVisitRequests",
                newName: "IX_LocationVisitRequests_RequestedById");

            migrationBuilder.AlterColumn<int>(
                name: "LocationBookingRequestId",
                table: "LocationVisitRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LocationVisitRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests",
                column: "RequestedById",
                principalTable: "LocationManagerProfiles",
                principalColumn: "Id");
        }
    }
}
