using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLocationOwnerFromLocationVisitRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationVisitRequests_LocationOwnerProfiles_LocationOwnerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropIndex(
                name: "IX_LocationVisitRequests_LocationOwnerId",
                table: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "LocationOwnerId",
                table: "LocationVisitRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationOwnerId",
                table: "LocationVisitRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationOwnerId",
                table: "LocationVisitRequests",
                column: "LocationOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationVisitRequests_LocationOwnerProfiles_LocationOwnerId",
                table: "LocationVisitRequests",
                column: "LocationOwnerId",
                principalTable: "LocationOwnerProfiles",
                principalColumn: "Id");
        }
    }
}
