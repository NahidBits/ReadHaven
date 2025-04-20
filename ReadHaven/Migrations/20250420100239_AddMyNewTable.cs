using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadHaven.Migrations
{
    /// <inheritdoc />
    public partial class AddMyNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*  migrationBuilder.DropColumn(
                  name: "Likes",
                  table: "Books");

              migrationBuilder.AddColumn<string>(
                  name: "ImagePath",
                  table: "Books",
                  type: "nvarchar(max)",
                  nullable: true);

              migrationBuilder.CreateTable(
                  name: "Orders",
                  columns: table => new
                  {
                      Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                      UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                      OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                      TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                      Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                      CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                      UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                      IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                  },
            constraints: table =>
                    {
                        table.PrimaryKey("PK_Orders", x => x.Id);
                    });*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
