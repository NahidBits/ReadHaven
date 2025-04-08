using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadHaven.Migrations
{
    /// <inheritdoc />
    public partial class update_review : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookRating",
                table: "BookReviews",
                newName: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "BookReviews",
                newName: "BookRating");
        }
    }
}
