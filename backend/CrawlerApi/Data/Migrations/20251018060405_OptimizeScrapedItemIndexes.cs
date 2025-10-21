using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlerApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeScrapedItemIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedItems_CollectedAt_Desc",
                table: "ScrapedItems",
                column: "CollectedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedItems_CollectedAt_Source",
                table: "ScrapedItems",
                columns: new[] { "CollectedAt", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedItems_Title",
                table: "ScrapedItems",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScrapedItems_CollectedAt_Desc",
                table: "ScrapedItems");

            migrationBuilder.DropIndex(
                name: "IX_ScrapedItems_CollectedAt_Source",
                table: "ScrapedItems");

            migrationBuilder.DropIndex(
                name: "IX_ScrapedItems_Title",
                table: "ScrapedItems");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
