using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class addedcustomservicetypeToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.RenameColumn(
                name: "LocationOnGoogleMaps",
                table: "RequestToLocationManagerToBookService",
                newName: "CustomServiceType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomServiceType",
                table: "RequestToLocationManagerToBookService",
                newName: "LocationOnGoogleMaps");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "RequestToLocationManagerToBookService",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "RequestToLocationManagerToBookService",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
