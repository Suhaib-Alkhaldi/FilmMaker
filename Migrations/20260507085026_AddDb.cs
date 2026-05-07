using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lockupCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lockupCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locationManagerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PreviousProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locationManagerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_locationManagerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locationOwnerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locationOwnerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_locationOwnerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productionCompanyProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productionCompanyProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_productionCompanyProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "serviceProviderProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serviceProviderProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_serviceProviderProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lockupItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LockupCategoryId = table.Column<int>(type: "int", nullable: false),
                    ProductionCompanyProfileId = table.Column<int>(type: "int", nullable: true),
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lockupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lockupItems_lockupCategories_LockupCategoryId",
                        column: x => x.LockupCategoryId,
                        principalTable: "lockupCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lockupItems_productionCompanyProfiles_ProductionCompanyProfileId",
                        column: x => x.ProductionCompanyProfileId,
                        principalTable: "productionCompanyProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_lockupItems_serviceProviderProfiles_ServiceProviderProfileId",
                        column: x => x.ServiceProviderProfileId,
                        principalTable: "serviceProviderProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "digitalContract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContractStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractStatusId = table.Column<int>(type: "int", nullable: false),
                    BookingStatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_digitalContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_digitalContract_lockupItems_BookingStatusId",
                        column: x => x.BookingStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_digitalContract_lockupItems_ContractStatusId",
                        column: x => x.ContractStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "locationBookingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingRequestStatusId = table.Column<int>(type: "int", nullable: false),
                    ShootingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationOwnerId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: false),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locationBookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_locationBookingRequests_locationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "locationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_locationBookingRequests_locationOwnerProfiles_LocationOwnerId",
                        column: x => x.LocationOwnerId,
                        principalTable: "locationOwnerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_locationBookingRequests_lockupItems_BookingRequestStatusId",
                        column: x => x.BookingRequestStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_locationBookingRequests_productionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "productionCompanyProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LocationOwnerId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: true),
                    LocationStatusId = table.Column<int>(type: "int", nullable: false),
                    LocationOnGoogleMaps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_locations_locationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "locationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_locations_locationOwnerProfiles_LocationOwnerId",
                        column: x => x.LocationOwnerId,
                        principalTable: "locationOwnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_locations_lockupItems_LocationStatusId",
                        column: x => x.LocationStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    PaymentStatusId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_locationBookingRequests_BookingId",
                        column: x => x.BookingId,
                        principalTable: "locationBookingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_payments_lockupItems_PaymentStatusId",
                        column: x => x.PaymentStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_payments_lockupItems_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "previousLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationStatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_previousLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_previousLocations_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_previousLocations_lockupItems_LocationStatusId",
                        column: x => x.LocationStatusId,
                        principalTable: "lockupItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_digitalContract_BookingStatusId",
                table: "digitalContract",
                column: "BookingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_digitalContract_ContractStatusId",
                table: "digitalContract",
                column: "ContractStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_locationBookingRequests_BookingRequestStatusId",
                table: "locationBookingRequests",
                column: "BookingRequestStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_locationBookingRequests_LocationManagerId",
                table: "locationBookingRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_locationBookingRequests_LocationOwnerId",
                table: "locationBookingRequests",
                column: "LocationOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_locationBookingRequests_ProductionCompanyId",
                table: "locationBookingRequests",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_locationManagerProfiles_UserId",
                table: "locationManagerProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_locationOwnerProfiles_UserId",
                table: "locationOwnerProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_locations_LocationManagerId",
                table: "locations",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_locations_LocationOwnerId",
                table: "locations",
                column: "LocationOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_locations_LocationStatusId",
                table: "locations",
                column: "LocationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_lockupItems_LockupCategoryId",
                table: "lockupItems",
                column: "LockupCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_lockupItems_ProductionCompanyProfileId",
                table: "lockupItems",
                column: "ProductionCompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_lockupItems_ServiceProviderProfileId",
                table: "lockupItems",
                column: "ServiceProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_BookingId",
                table: "payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentStatusId",
                table: "payments",
                column: "PaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentTypeId",
                table: "payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_previousLocations_LocationId",
                table: "previousLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_previousLocations_LocationStatusId",
                table: "previousLocations",
                column: "LocationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_productionCompanyProfiles_UserId",
                table: "productionCompanyProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_serviceProviderProfiles_UserId",
                table: "serviceProviderProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "digitalContract");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "previousLocations");

            migrationBuilder.DropTable(
                name: "locationBookingRequests");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "locationManagerProfiles");

            migrationBuilder.DropTable(
                name: "locationOwnerProfiles");

            migrationBuilder.DropTable(
                name: "lockupItems");

            migrationBuilder.DropTable(
                name: "lockupCategories");

            migrationBuilder.DropTable(
                name: "productionCompanyProfiles");

            migrationBuilder.DropTable(
                name: "serviceProviderProfiles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
