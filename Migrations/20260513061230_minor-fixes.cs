using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmMaker.Migrations
{
    /// <inheritdoc />
    public partial class minorfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicesProvided",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "smallmoney", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_ServiceProviderProfiles_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicesProvidedMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    ServicesProvidedId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "IX_ServicesProvided_ServiceProviderId",
                table: "ServicesProvided",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_ServiceTypeId",
                table: "ServicesProvided",
                column: "ServiceTypeId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicesProvidedMedia");

            migrationBuilder.DropTable(
                name: "ServicesProvided");
        }
    }
}
