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
            migrationBuilder.AlterColumn<bool>(
                name: "IsRestored",
                table: "LocationArchiveHistories",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "ArchivedByUserId",
                table: "LocationArchiveHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArchivedAt",
                table: "LocationArchiveHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.InsertData(
                table: "LockupCategories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(957), null, true, false, "LocationStatus", null, null });

            migrationBuilder.InsertData(
                table: "LockupItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "LockupCategoryId", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1106), null, true, false, 1, "Published", null, null },
                    { 2, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1107), null, true, false, 1, "Blocked", null, null },
                    { 3, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1108), null, true, false, 1, "Deleted", null, null },
                    { 4, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1109), null, true, false, 1, "Under Moderation", null, null },
                    { 5, new DateTime(2026, 5, 10, 12, 34, 56, 851, DateTimeKind.Local).AddTicks(1110), null, true, false, 1, "Archived", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LockupItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LockupCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRestored",
                table: "LocationArchiveHistories",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArchivedByUserId",
                table: "LocationArchiveHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArchivedAt",
                table: "LocationArchiveHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
