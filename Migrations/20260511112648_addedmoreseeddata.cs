using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedmoreseeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LockupCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(7953));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8108));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8109));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8110));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8111));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8132), null, true, false, "Admin", null, null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "IBAN", "IsActive", "IsDeleted", "Name", "Password", "PhoneNumber", "RoleId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8177), null, "alice.johnson@example.com", "US12345678901234567890", true, false, "Alice Johnson", "HashedPassword123!", "+1-555-0101", 1, null, null });

            migrationBuilder.InsertData(
                table: "LocationOwnerProfiles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "RegisterDate", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, new DateTime(2026, 5, 11, 14, 26, 47, 119, DateTimeKind.Local).AddTicks(8154), null, true, false, new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LocationOwnerProfiles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "LockupCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(957));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1106));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1107));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1108));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1109));

            migrationBuilder.UpdateData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1110));
        }
    }
}
