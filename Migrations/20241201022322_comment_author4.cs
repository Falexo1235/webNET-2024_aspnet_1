using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Migrations
{
    /// <inheritdoc />
    public partial class comment_author4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuthorFullName",
                table: "Posts",
                newName: "Author");

            migrationBuilder.RenameColumn(
                name: "AuthorFullName",
                table: "Comments",
                newName: "Author");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Posts",
                newName: "AuthorFullName");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Comments",
                newName: "AuthorFullName");
        }
    }
}
