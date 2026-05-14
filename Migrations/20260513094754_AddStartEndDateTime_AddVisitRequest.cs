using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddStartEndDateTime_AddVisitRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShootingDate",
                table: "LocationBookingRequests",
                newName: "StartDateTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "LocationBookingRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "LocationVisitRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationBookingRequestId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    VisitDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                        name: "FK_LocationVisitRequests_LocationBookingRequests_LocationBookingRequestId",
                        column: x => x.LocationBookingRequestId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_LocationManagerProfiles_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationBookingRequestId",
                table: "LocationVisitRequests",
                column: "LocationBookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationId",
                table: "LocationVisitRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_RequestedById",
                table: "LocationVisitRequests",
                column: "RequestedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationVisitRequests");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "LocationBookingRequests");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "LocationBookingRequests",
                newName: "ShootingDate");
        }
    }
}
