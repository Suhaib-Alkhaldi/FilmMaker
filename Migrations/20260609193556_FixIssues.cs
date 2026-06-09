using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class FixIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicesProvidedMedia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicesProvidedMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaTypeId = table.Column<int>(type: "int", nullable: false),
                    ServicesProvidedId = table.Column<int>(type: "int", nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesProvidedMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesProvidedMedia_LookupItems_MediaTypeId",
                        column: x => x.MediaTypeId,
                        principalTable: "LookupItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicesProvidedMedia_ServicesProvided_ServicesProvidedId",
                        column: x => x.ServicesProvidedId,
                        principalTable: "ServicesProvided",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicesProvidedMedia_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvidedMedia_MediaTypeId",
                table: "ServicesProvidedMedia",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvidedMedia_ServicesProvidedId",
                table: "ServicesProvidedMedia",
                column: "ServicesProvidedId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvidedMedia_UploadedByUserId",
                table: "ServicesProvidedMedia",
                column: "UploadedByUserId");
        }
    }
}
