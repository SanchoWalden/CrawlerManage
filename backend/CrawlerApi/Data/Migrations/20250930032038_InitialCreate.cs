using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlerApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScrapedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    MetadataJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedItems_Source",
                table: "ScrapedItems",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedItems_Url",
                table: "ScrapedItems",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapedItems");
        }
    }
}
