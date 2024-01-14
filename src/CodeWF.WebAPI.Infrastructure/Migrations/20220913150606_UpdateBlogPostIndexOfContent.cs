using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeWF.WebAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBlogPostIndexOfContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppBlogPosts_Content",
                table: "AppBlogPosts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppBlogPosts_Content",
                table: "AppBlogPosts",
                column: "Content");
        }
    }
}
