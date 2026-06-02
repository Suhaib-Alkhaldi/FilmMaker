using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddedRequestToLocationManagerToBookService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestToLocationManagerToBookService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    LocationBookingId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestToLocationManagerToBookService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_LocationBookingRequests_LocationBookingId",
                        column: x => x.LocationBookingId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_LookupItems_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_ProductionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_LocationBookingId",
                table: "RequestToLocationManagerToBookService",
                column: "LocationBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_ProductionCompanyId",
                table: "RequestToLocationManagerToBookService",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_ServiceTypeId",
                table: "RequestToLocationManagerToBookService",
                column: "ServiceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestToLocationManagerToBookService");
        }
    }
}
