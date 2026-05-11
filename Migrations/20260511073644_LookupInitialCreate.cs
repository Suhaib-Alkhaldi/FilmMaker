using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class LookupInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingStatusHistories_LockupItems_FromStatusId",
                table: "BookingStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingStatusHistories_LockupItems_ToStatusId",
                table: "BookingStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_DigitalContracts_LockupItems_ContractStatusId",
                table: "DigitalContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_EscrowTransactions_LockupItems_EscrowStatusId",
                table: "EscrowTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationBookingRequests_LockupItems_BookingStatusId",
                table: "LocationBookingRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationManagerCities_LockupItems_CityId",
                table: "LocationManagerCities");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationMedia_LockupItems_MediaTypeId",
                table: "LocationMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_LockupItems_LocationStatusId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LockupItems_PaymentStatusId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LockupItems_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionCompanyProductionTypes_LockupItems_ProductionTypeId",
                table: "ProductionCompanyProductionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceProviderServiceTypes_LockupItems_ServiceTypeId",
                table: "ServiceProviderServiceTypes");

            migrationBuilder.DropTable(
                name: "LockupItems");

            migrationBuilder.DropTable(
                name: "LockupCategories");

            migrationBuilder.CreateTable(
                name: "LookupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LookupCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupItems_LookupCategories_LookupCategoryId",
                        column: x => x.LookupCategoryId,
                        principalTable: "LookupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupItems_LookupCategoryId",
                table: "LookupItems",
                column: "LookupCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingStatusHistories_LookupItems_FromStatusId",
                table: "BookingStatusHistories",
                column: "FromStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingStatusHistories_LookupItems_ToStatusId",
                table: "BookingStatusHistories",
                column: "ToStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalContracts_LookupItems_ContractStatusId",
                table: "DigitalContracts",
                column: "ContractStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscrowTransactions_LookupItems_EscrowStatusId",
                table: "EscrowTransactions",
                column: "EscrowStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationBookingRequests_LookupItems_BookingStatusId",
                table: "LocationBookingRequests",
                column: "BookingStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationManagerCities_LookupItems_CityId",
                table: "LocationManagerCities",
                column: "CityId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationMedia_LookupItems_MediaTypeId",
                table: "LocationMedia",
                column: "MediaTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_LookupItems_LocationStatusId",
                table: "Locations",
                column: "LocationStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LookupItems_PaymentStatusId",
                table: "Payments",
                column: "PaymentStatusId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LookupItems_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionCompanyProductionTypes_LookupItems_ProductionTypeId",
                table: "ProductionCompanyProductionTypes",
                column: "ProductionTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceProviderServiceTypes_LookupItems_ServiceTypeId",
                table: "ServiceProviderServiceTypes",
                column: "ServiceTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingStatusHistories_LookupItems_FromStatusId",
                table: "BookingStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingStatusHistories_LookupItems_ToStatusId",
                table: "BookingStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_DigitalContracts_LookupItems_ContractStatusId",
                table: "DigitalContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_EscrowTransactions_LookupItems_EscrowStatusId",
                table: "EscrowTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationBookingRequests_LookupItems_BookingStatusId",
                table: "LocationBookingRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationManagerCities_LookupItems_CityId",
                table: "LocationManagerCities");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationMedia_LookupItems_MediaTypeId",
                table: "LocationMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_LookupItems_LocationStatusId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LookupItems_PaymentStatusId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LookupItems_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionCompanyProductionTypes_LookupItems_ProductionTypeId",
                table: "ProductionCompanyProductionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceProviderServiceTypes_LookupItems_ServiceTypeId",
                table: "ServiceProviderServiceTypes");

            migrationBuilder.DropTable(
                name: "LookupItems");

            migrationBuilder.DropTable(
                name: "LookupCategories");

            migrationBuilder.CreateTable(
                name: "LockupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockupCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LockupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LockupCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockupItems_LockupCategories_LockupCategoryId",
                        column: x => x.LockupCategoryId,
                        principalTable: "LockupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockupItems_LockupCategoryId",
                table: "LockupItems",
                column: "LockupCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingStatusHistories_LockupItems_FromStatusId",
                table: "BookingStatusHistories",
                column: "FromStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingStatusHistories_LockupItems_ToStatusId",
                table: "BookingStatusHistories",
                column: "ToStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalContracts_LockupItems_ContractStatusId",
                table: "DigitalContracts",
                column: "ContractStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EscrowTransactions_LockupItems_EscrowStatusId",
                table: "EscrowTransactions",
                column: "EscrowStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationBookingRequests_LockupItems_BookingStatusId",
                table: "LocationBookingRequests",
                column: "BookingStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationManagerCities_LockupItems_CityId",
                table: "LocationManagerCities",
                column: "CityId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationMedia_LockupItems_MediaTypeId",
                table: "LocationMedia",
                column: "MediaTypeId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_LockupItems_LocationStatusId",
                table: "Locations",
                column: "LocationStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LockupItems_PaymentStatusId",
                table: "Payments",
                column: "PaymentStatusId",
                principalTable: "LockupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LockupItems_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId",
                principalTable: "LockupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionCompanyProductionTypes_LockupItems_ProductionTypeId",
                table: "ProductionCompanyProductionTypes",
                column: "ProductionTypeId",
                principalTable: "LockupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceProviderServiceTypes_LockupItems_ServiceTypeId",
                table: "ServiceProviderServiceTypes",
                column: "ServiceTypeId",
                principalTable: "LockupItems",
                principalColumn: "Id");
        }
    }
}
