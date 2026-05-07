using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedLocationTermsOfUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "locationBookingRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "locationTermsOfUse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locationTermsOfUse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_locationTermsOfUse_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_locationBookingRequests_LocationId",
                table: "locationBookingRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_locationTermsOfUse_LocationId",
                table: "locationTermsOfUse",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_locationBookingRequests_locations_LocationId",
                table: "locationBookingRequests",
                column: "LocationId",
                principalTable: "locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_locationBookingRequests_locations_LocationId",
                table: "locationBookingRequests");

            migrationBuilder.DropTable(
                name: "locationTermsOfUse");

            migrationBuilder.DropIndex(
                name: "IX_locationBookingRequests_LocationId",
                table: "locationBookingRequests");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "locationBookingRequests");
        }
    }
}
