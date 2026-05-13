using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedLocationBackToServiceBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "ServiceBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_LocationId",
                table: "ServiceBookings",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Locations_LocationId",
                table: "ServiceBookings",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_Locations_LocationId",
                table: "ServiceBookings");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_LocationId",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ServiceBookings");
        }
    }
}
