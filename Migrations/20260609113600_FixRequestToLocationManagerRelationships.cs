using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class FixRequestToLocationManagerRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LocationBookingRequests_LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropIndex(
                name: "IX_RequestToLocationManagerToBookService_LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService",
                column: "LocationBookingRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LocationBookingRequests_LocationBookingRequestId",
                table: "RequestToLocationManagerToBookService",
                column: "LocationBookingRequestId",
                principalTable: "LocationBookingRequests",
                principalColumn: "Id");
        }
    }
}
