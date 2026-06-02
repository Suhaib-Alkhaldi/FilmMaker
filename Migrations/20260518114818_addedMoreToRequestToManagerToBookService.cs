using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedMoreToRequestToManagerToBookService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RequestToLocationManagerToBookService",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "RequestToLocationManagerToBookService",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationOnGoogleMaps",
                table: "RequestToLocationManagerToBookService",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "RequestToLocationManagerToBookService",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "LocationOnGoogleMaps",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RequestToLocationManagerToBookService",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
