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
                name: "Roles",
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
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "LocationManagerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationManagerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationManagerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationOwnerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOwnerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOwnerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    MediaTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_LookupItems_MediaTypeId",
                        column: x => x.MediaTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionCompanyProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCompanyProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionCompanyProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationManagerCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationManagerProfileId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationManagerCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationManagerCities_LocationManagerProfiles_LocationManagerProfileId",
                        column: x => x.LocationManagerProfileId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationManagerCities_LookupItems_CityId",
                        column: x => x.CityId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreviousProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationManagerProfileId = table.Column<int>(type: "int", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreviousProjects_LocationManagerProfiles_LocationManagerProfileId",
                        column: x => x.LocationManagerProfileId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DailyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LocationOwnerId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: true),
                    LocationStatusId = table.Column<int>(type: "int", nullable: false),
                    LocationTypeId = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FacilitiesDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationOnGoogleMaps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Locations_LocationOwnerProfiles_LocationOwnerId",
                        column: x => x.LocationOwnerId,
                        principalTable: "LocationOwnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Locations_LookupItems_LocationStatusId",
                        column: x => x.LocationStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Locations_LookupItems_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateTable(
                name: "ProductionCompanyProductionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionCompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    ProductionTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCompanyProductionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionCompanyProductionTypes_LookupItems_ProductionTypeId",
                        column: x => x.ProductionTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionCompanyProductionTypes_ProductionCompanyProfiles_ProductionCompanyProfileId",
                        column: x => x.ProductionCompanyProfileId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderCities_LookupItems_CityId",
                        column: x => x.CityId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceProviderCities_ServiceProviderProfiles_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderServiceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: true),
                    CustomServiceTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderServiceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderServiceTypes_LookupItems_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceProviderServiceTypes_ServiceProviderProfiles_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicesProvided",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "smallmoney", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: true),
                    CustomServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesProvided", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_LookupItems_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicesProvided_ServiceProviderProfiles_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationArchiveHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ArchivedByUserId = table.Column<int>(type: "int", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRestored = table.Column<bool>(type: "bit", nullable: false),
                    RestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationArchiveHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationArchiveHistories_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationArchiveHistories_Users_ArchivedByUserId",
                        column: x => x.ArchivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationArchiveHistories_Users_RestoredByUserId",
                        column: x => x.RestoredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMedia_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationMedia_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationTermsOfUse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    TermText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTermsOfUse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationTermsOfUse_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationVisitRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: true),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
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
                    table.CheckConstraint("CK_LocationVisitRequest_SingleRequesterProfile", "    (\r\n        ([LocationManagerId] IS NOT NULL AND [ProductionCompanyId] IS NULL)\r\n        OR\r\n        ([LocationManagerId] IS NULL AND [ProductionCompanyId] IS NOT NULL)\r\n    )");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
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
                        name: "FK_LocationVisitRequests_ProductionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationVisitRequests_Users_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationBookingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    BookingStatusId = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationOwnerId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: true),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    LocationScoutingRequestId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationBookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_LocationOwnerProfiles_LocationOwnerId",
                        column: x => x.LocationOwnerId,
                        principalTable: "LocationOwnerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_LocationScoutingRequests_LocationScoutingRequestId",
                        column: x => x.LocationScoutingRequestId,
                        principalTable: "LocationScoutingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_LookupItems_BookingStatusId",
                        column: x => x.BookingStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationBookingRequests_ProductionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServicesMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicesProvidedId = table.Column<int>(type: "int", nullable: false),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesMedia_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicesMedia_ServicesProvided_ServicesProvidedId",
                        column: x => x.ServicesProvidedId,
                        principalTable: "ServicesProvided",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationBookingRequestId = table.Column<int>(type: "int", nullable: false),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistories_LocationBookingRequests_LocationBookingRequestId",
                        column: x => x.LocationBookingRequestId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingStatusHistories_LookupItems_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingStatusHistories_LookupItems_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DigitalContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationBookingRequestId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermsSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContractStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractStatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalContracts_LocationBookingRequests_LocationBookingRequestId",
                        column: x => x.LocationBookingRequestId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalContracts_LookupItems_ContractStatusId",
                        column: x => x.ContractStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestToLocationManagerToBookService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: false),
                    LocationBookingId = table.Column<int>(type: "int", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_RequestToLocationManagerToBookService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_LocationBookingRequests_LocationBookingId",
                        column: x => x.LocationBookingId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_LookupItems_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookService_ProductionCompanyProfiles_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanyProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DigitalContractApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DigitalContractId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalContractApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalContractApprovals_DigitalContracts_DigitalContractId",
                        column: x => x.DigitalContractId,
                        principalTable: "DigitalContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalContractApprovals_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationBookingRequestId = table.Column<int>(type: "int", nullable: false),
                    DigitalContractId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatusId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_DigitalContracts_DigitalContractId",
                        column: x => x.DigitalContractId,
                        principalTable: "DigitalContracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_LocationBookingRequests_LocationBookingRequestId",
                        column: x => x.LocationBookingRequestId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_LookupItems_PaymentStatusId",
                        column: x => x.PaymentStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_LookupItems_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestToLocationManagerToBookServiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestToLocationManagerToBookServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: true),
                    CustomServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestToLocationManagerToBookServiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookServiceItems_LookupItems_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequestToLocationManagerToBookServiceItems_RequestToLocationManagerToBookService_RequestToLocationManagerToBookServiceId",
                        column: x => x.RequestToLocationManagerToBookServiceId,
                        principalTable: "RequestToLocationManagerToBookService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestToLocationManagerToBookServiceId = table.Column<int>(type: "int", nullable: false),
                    LocationManagerId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
                    MessageToProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ServiceProviderRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequests_LocationManagerProfiles_LocationManagerId",
                        column: x => x.LocationManagerId,
                        principalTable: "LocationManagerProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequests_LookupItems_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequests_RequestToLocationManagerToBookService_RequestToLocationManagerToBookServiceId",
                        column: x => x.RequestToLocationManagerToBookServiceId,
                        principalTable: "RequestToLocationManagerToBookService",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequests_ServiceProviderProfiles_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EscrowTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    LocationBookingRequestId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EscrowStatusId = table.Column<int>(type: "int", nullable: false),
                    HeldAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_LocationBookingRequests_LocationBookingRequestId",
                        column: x => x.LocationBookingRequestId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_LookupItems_EscrowStatusId",
                        column: x => x.EscrowStatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderRequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderRequestId = table.Column<int>(type: "int", nullable: false),
                    RequestToLocationManagerToBookServiceItemId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequestItems_RequestToLocationManagerToBookServiceItems_RequestToLocationManagerToBookServiceItemId",
                        column: x => x.RequestToLocationManagerToBookServiceItemId,
                        principalTable: "RequestToLocationManagerToBookServiceItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequestItems_ServiceProviderRequests_ServiceProviderRequestId",
                        column: x => x.ServiceProviderRequestId,
                        principalTable: "ServiceProviderRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceProviderRequestItems_ServicesProvided_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "ServicesProvided",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    LocationBookingId = table.Column<int>(type: "int", nullable: true),
                    ServiceProviderRequestItemId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    BookingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBookings_LocationBookingRequests_LocationBookingId",
                        column: x => x.LocationBookingId,
                        principalTable: "LocationBookingRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBookings_LookupItems_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBookings_ServiceProviderRequestItems_ServiceProviderRequestItemId",
                        column: x => x.ServiceProviderRequestItemId,
                        principalTable: "ServiceProviderRequestItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBookings_ServicesProvided_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "ServicesProvided",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBookings_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistories_ChangedByUserId",
                table: "BookingStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistories_FromStatusId",
                table: "BookingStatusHistories",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistories_LocationBookingRequestId",
                table: "BookingStatusHistories",
                column: "LocationBookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistories_ToStatusId",
                table: "BookingStatusHistories",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalContractApprovals_ApprovedByUserId",
                table: "DigitalContractApprovals",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalContractApprovals_DigitalContractId",
                table: "DigitalContractApprovals",
                column: "DigitalContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalContracts_ContractStatusId",
                table: "DigitalContracts",
                column: "ContractStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalContracts_LocationBookingRequestId",
                table: "DigitalContracts",
                column: "LocationBookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_EscrowStatusId",
                table: "EscrowTransactions",
                column: "EscrowStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_LocationBookingRequestId",
                table: "EscrowTransactions",
                column: "LocationBookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_PaymentId",
                table: "EscrowTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationArchiveHistories_ArchivedByUserId",
                table: "LocationArchiveHistories",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationArchiveHistories_LocationId",
                table: "LocationArchiveHistories",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationArchiveHistories_RestoredByUserId",
                table: "LocationArchiveHistories",
                column: "RestoredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_BookingStatusId",
                table: "LocationBookingRequests",
                column: "BookingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_LocationId",
                table: "LocationBookingRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_LocationManagerId",
                table: "LocationBookingRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_LocationOwnerId",
                table: "LocationBookingRequests",
                column: "LocationOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_LocationScoutingRequestId",
                table: "LocationBookingRequests",
                column: "LocationScoutingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationBookingRequests_ProductionCompanyId",
                table: "LocationBookingRequests",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationManagerCities_CityId",
                table: "LocationManagerCities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationManagerCities_LocationManagerProfileId",
                table: "LocationManagerCities",
                column: "LocationManagerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationManagerProfiles_UserId",
                table: "LocationManagerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationMedia_LocationId",
                table: "LocationMedia",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationMedia_MediaId",
                table: "LocationMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOwnerProfiles_UserId",
                table: "LocationOwnerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationManagerId",
                table: "Locations",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationOwnerId",
                table: "Locations",
                column: "LocationOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationStatusId",
                table: "Locations",
                column: "LocationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationTypeId",
                table: "Locations",
                column: "LocationTypeId");

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

            migrationBuilder.CreateIndex(
                name: "IX_LocationTermsOfUse_LocationId",
                table: "LocationTermsOfUse",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationId",
                table: "LocationVisitRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_LocationManagerId",
                table: "LocationVisitRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_ProductionCompanyId",
                table: "LocationVisitRequests",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_RequestedByUserId",
                table: "LocationVisitRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_RespondedByUserId",
                table: "LocationVisitRequests",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationVisitRequests_VisitStatusId",
                table: "LocationVisitRequests",
                column: "VisitStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupItems_LookupCategoryId",
                table: "LookupItems",
                column: "LookupCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_MediaTypeId",
                table: "Media",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_UploadedByUserId",
                table: "Media",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_UserId",
                table: "OtpCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DigitalContractId",
                table: "Payments",
                column: "DigitalContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LocationBookingRequestId",
                table: "Payments",
                column: "LocationBookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentStatusId",
                table: "Payments",
                column: "PaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousProjects_LocationManagerProfileId",
                table: "PreviousProjects",
                column: "LocationManagerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCompanyProductionTypes_ProductionCompanyProfileId",
                table: "ProductionCompanyProductionTypes",
                column: "ProductionCompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCompanyProductionTypes_ProductionTypeId",
                table: "ProductionCompanyProductionTypes",
                column: "ProductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCompanyProfiles_UserId",
                table: "ProductionCompanyProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_LocationBookingId",
                table: "RequestToLocationManagerToBookService",
                column: "LocationBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_LocationManagerId",
                table: "RequestToLocationManagerToBookService",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_ProductionCompanyId",
                table: "RequestToLocationManagerToBookService",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_StatusId",
                table: "RequestToLocationManagerToBookService",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookServiceItems_RequestToLocationManagerToBookServiceId",
                table: "RequestToLocationManagerToBookServiceItems",
                column: "RequestToLocationManagerToBookServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookServiceItems_ServiceTypeId",
                table: "RequestToLocationManagerToBookServiceItems",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_LocationBookingId",
                table: "ServiceBookings",
                column: "LocationBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_RequesterId",
                table: "ServiceBookings",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_ServiceId",
                table: "ServiceBookings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_ServiceProviderRequestItemId",
                table: "ServiceBookings",
                column: "ServiceProviderRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_StatusId",
                table: "ServiceBookings",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderCities_CityId",
                table: "ServiceProviderCities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderCities_ServiceProviderId",
                table: "ServiceProviderCities",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderProfiles_UserId",
                table: "ServiceProviderProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequestItems_RequestToLocationManagerToBookServiceItemId",
                table: "ServiceProviderRequestItems",
                column: "RequestToLocationManagerToBookServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequestItems_ServiceId",
                table: "ServiceProviderRequestItems",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequestItems_ServiceProviderRequestId",
                table: "ServiceProviderRequestItems",
                column: "ServiceProviderRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequests_LocationManagerId",
                table: "ServiceProviderRequests",
                column: "LocationManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequests_RequestToLocationManagerToBookServiceId",
                table: "ServiceProviderRequests",
                column: "RequestToLocationManagerToBookServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequests_ServiceProviderId",
                table: "ServiceProviderRequests",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderRequests_StatusId",
                table: "ServiceProviderRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderServiceTypes_ServiceProviderId",
                table: "ServiceProviderServiceTypes",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderServiceTypes_ServiceTypeId",
                table: "ServiceProviderServiceTypes",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesMedia_MediaId",
                table: "ServicesMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesMedia_ServicesProvidedId",
                table: "ServicesMedia",
                column: "ServicesProvidedId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_ServiceProviderId",
                table: "ServicesProvided",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_ServiceTypeId",
                table: "ServicesProvided",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingStatusHistories");

            migrationBuilder.DropTable(
                name: "DigitalContractApprovals");

            migrationBuilder.DropTable(
                name: "EscrowTransactions");

            migrationBuilder.DropTable(
                name: "LocationArchiveHistories");

            migrationBuilder.DropTable(
                name: "LocationManagerCities");

            migrationBuilder.DropTable(
                name: "LocationMedia");

            migrationBuilder.DropTable(
                name: "LocationTermsOfUse");

            migrationBuilder.DropTable(
                name: "LocationVisitRequests");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "PreviousProjects");

            migrationBuilder.DropTable(
                name: "ProductionCompanyProductionTypes");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ServiceBookings");

            migrationBuilder.DropTable(
                name: "ServiceProviderCities");

            migrationBuilder.DropTable(
                name: "ServiceProviderServiceTypes");

            migrationBuilder.DropTable(
                name: "ServicesMedia");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ServiceProviderRequestItems");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "DigitalContracts");

            migrationBuilder.DropTable(
                name: "RequestToLocationManagerToBookServiceItems");

            migrationBuilder.DropTable(
                name: "ServiceProviderRequests");

            migrationBuilder.DropTable(
                name: "ServicesProvided");

            migrationBuilder.DropTable(
                name: "RequestToLocationManagerToBookService");

            migrationBuilder.DropTable(
                name: "ServiceProviderProfiles");

            migrationBuilder.DropTable(
                name: "LocationBookingRequests");

            migrationBuilder.DropTable(
                name: "LocationScoutingRequests");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "ProductionCompanyProfiles");

            migrationBuilder.DropTable(
                name: "LocationManagerProfiles");

            migrationBuilder.DropTable(
                name: "LocationOwnerProfiles");

            migrationBuilder.DropTable(
                name: "LookupItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LookupCategories");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
