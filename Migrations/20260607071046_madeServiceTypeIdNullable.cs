using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class madeServiceTypeIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_ServiceTypeId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceTypeId",
                table: "RequestToLocationManagerToBookService",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_ServiceTypeId",
                table: "RequestToLocationManagerToBookService",
                column: "ServiceTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_ServiceTypeId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceTypeId",
                table: "RequestToLocationManagerToBookService",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_ServiceTypeId",
                table: "RequestToLocationManagerToBookService",
                column: "ServiceTypeId",
                principalTable: "LookupItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
