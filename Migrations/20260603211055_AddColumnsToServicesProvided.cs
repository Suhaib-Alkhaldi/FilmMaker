using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToServicesProvided : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicesProvided_LookupItems_ServiceTypeId",
                table: "ServicesProvided");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceTypeId",
                table: "ServicesProvided",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CustomServiceType",
                table: "ServicesProvided",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "ServicesProvided",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesProvided_LookupItems_ServiceTypeId",
                table: "ServicesProvided",
                column: "ServiceTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicesProvided_LookupItems_ServiceTypeId",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "CustomServiceType",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "ServicesProvided");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceTypeId",
                table: "ServicesProvided",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicesProvided_LookupItems_ServiceTypeId",
                table: "ServicesProvided",
                column: "ServiceTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
