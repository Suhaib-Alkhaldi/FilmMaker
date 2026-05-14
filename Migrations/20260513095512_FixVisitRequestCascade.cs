using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class FixVisitRequestCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationBookingRequests_LocationBookingRequestId",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_Locations_LocationId",
                table: "LocationVisitRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationBookingRequests_LocationBookingRequestId",
                table: "LocationVisitRequests",
                column: "LocationBookingRequestId",
                principalTable: "LocationBookingRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests",
                column: "RequestedById",
                principalTable: "LocationManagerProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_Locations_LocationId",
                table: "LocationVisitRequests",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationBookingRequests_LocationBookingRequestId",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_Locations_LocationId",
                table: "LocationVisitRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationBookingRequests_LocationBookingRequestId",
                table: "LocationVisitRequests",
                column: "LocationBookingRequestId",
                principalTable: "LocationBookingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                table: "LocationVisitRequests",
                column: "RequestedById",
                principalTable: "LocationManagerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_Locations_LocationId",
                table: "LocationVisitRequests",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
