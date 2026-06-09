using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationBookingRequestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductionCompanyId",
                table: "LocationBookingRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationScoutingRequestId",
                table: "LocationBookingRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_LocationScoutingRequestId",
                table: "LocationBookingRequests",
                column: "LocationScoutingRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationBookingRequests_LocationScoutingRequests_LocationScoutingRequestId",
                table: "LocationBookingRequests",
                column: "LocationScoutingRequestId",
                principalTable: "LocationScoutingRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationBookingRequests_LocationScoutingRequests_LocationScoutingRequestId",
                table: "LocationBookingRequests");

            migrationBuilder.DropIndex(
                name: "IX_LocationBookingRequests_LocationScoutingRequestId",
                table: "LocationBookingRequests");

            migrationBuilder.DropColumn(
                name: "LocationScoutingRequestId",
                table: "LocationBookingRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ProductionCompanyId",
                table: "LocationBookingRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
