using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadHaven.Migrations
{
    /// <inheritdoc />
    public partial class update_order_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First convert existing string values to enum integer values
            migrationBuilder.Sql("UPDATE Orders SET Status = 0 WHERE Status = 'Pending'");
            migrationBuilder.Sql("UPDATE Orders SET Status = 1 WHERE Status = 'Processing'");
            migrationBuilder.Sql("UPDATE Orders SET Status = 2 WHERE Status = 'Completed'");
            migrationBuilder.Sql("UPDATE Orders SET Status = 3 WHERE Status = 'Cancelled'");

            // Now alter the column from string to int
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back from int to string in case of rollback
            migrationBuilder.Sql("UPDATE Orders SET Status = 'Pending' WHERE Status = 0");
            migrationBuilder.Sql("UPDATE Orders SET Status = 'Processing' WHERE Status = 1");
            migrationBuilder.Sql("UPDATE Orders SET Status = 'Completed' WHERE Status = 2");
            migrationBuilder.Sql("UPDATE Orders SET Status = 'Cancelled' WHERE Status = 3");

            // Revert column to string
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
