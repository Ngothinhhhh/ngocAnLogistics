using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột Status với ENUM type
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "tblorder",
                type: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','Completed','Cancelled','FailedDelivery')",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback - chuyển về kiểu int
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "tblorder",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','Completed','Cancelled','FailedDelivery')",
                oldDefaultValue: "Pending");
        }
    }
}
