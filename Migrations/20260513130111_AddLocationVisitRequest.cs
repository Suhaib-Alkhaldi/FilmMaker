using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationVisitRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationVisitRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationOwnerId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: false),
                    RequestedVisitDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitStatusId = table.Column<int>(type: "int", nullable: false),
                    OwnerResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RespondedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationVisitRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_LocationOwnerProfiles_LocationOwnerId",
                        column: x => x.LocationOwnerId,
                        principalTable: "LocationOwnerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_LookupItems_VisitStatusId",
                        column: x => x.VisitStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_Users_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationId",
                table: "LocationVisitRequests",
                column: "LocationId");

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

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_VisitStatusId",
                table: "LocationVisitRequests",
                column: "VisitStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationVisitRequests");
        }
    }
}
