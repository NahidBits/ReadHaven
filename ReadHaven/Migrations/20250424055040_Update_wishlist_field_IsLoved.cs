using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadHaven.Migrations
{
    /// <inheritdoc />
    public partial class Update_wishlist_field_IsLoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLoved",
                table: "Wishlists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoved",
                table: "Wishlists");
        }
    }
}
