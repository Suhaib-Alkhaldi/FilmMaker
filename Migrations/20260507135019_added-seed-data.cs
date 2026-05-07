using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedseeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "Admin", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 2, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "LocationOwner", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 3, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "LocationManager", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 4, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "ProductionCompany", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 5, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "ServiceProvider", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.InsertData(
                table: "lockupCategories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, "LocationStatus", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "IBAN", "IsActive", "IsDeleted", "Name", "Password", "PhoneNumber", "RoleId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", "admin@filmmaker.com", "SA0000000000000000000000", true, false, "Admin User", "123", "0500000000", 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 2, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", "omar@owner.com", "SA1111111111111111111111", true, false, "Omar Owner", "HashedPasswordHere", "0511111111", 2, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.InsertData(
                table: "lockupItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "LockupCategoryId", "Name", "ProductionCompanyProfileId", "ServiceProviderProfileId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, 1, "Published", null, null, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { 2, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", false, false, 1, "Blocked", null, null, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.InsertData(
                table: "locationOwnerProfiles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "RegisterDate", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "System", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "locationOwnerProfiles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "lockupItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "lockupItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "lockupCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
