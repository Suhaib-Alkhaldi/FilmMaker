using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class FixRequestToLocationManagerShadowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_LookupItemId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestToLocationManagerToBookService_ProductionCompanyProfiles_ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropIndex(
                name: "IX_RequestToLocationManagerToBookService_LookupItemId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropIndex(
                name: "IX_RequestToLocationManagerToBookService_ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "LookupItemId",
                table: "RequestToLocationManagerToBookService");

            migrationBuilder.DropColumn(
                name: "ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LookupItemId",
                table: "RequestToLocationManagerToBookService",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_LookupItemId",
                table: "RequestToLocationManagerToBookService",
                column: "LookupItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestToLocationManagerToBookService_ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService",
                column: "ProductionCompanyProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestToLocationManagerToBookService_LookupItems_LookupItemId",
                table: "RequestToLocationManagerToBookService",
                column: "LookupItemId",
                principalTable: "LookupItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestToLocationManagerToBookService_ProductionCompanyProfiles_ProductionCompanyProfileId",
                table: "RequestToLocationManagerToBookService",
                column: "ProductionCompanyProfileId",
                principalTable: "ProductionCompanyProfiles",
                principalColumn: "Id");
        }
    }
}
