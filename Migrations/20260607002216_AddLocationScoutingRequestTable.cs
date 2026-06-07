using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationScoutingRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationScoutingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RespondedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationScoutingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationScoutingRequests_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationScoutingRequests_LookupItems_CityId",
                        column: x => x.CityId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationScoutingRequests_LookupItems_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationScoutingRequests_ProductionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationScoutingRequests_CityId",
                table: "LocationScoutingRequests",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationScoutingRequests_LocationManagerId",
                table: "LocationScoutingRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationScoutingRequests_ProductionCompanyId",
                table: "LocationScoutingRequests",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationScoutingRequests_StatusId",
                table: "LocationScoutingRequests",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationScoutingRequests");
        }
    }
}
