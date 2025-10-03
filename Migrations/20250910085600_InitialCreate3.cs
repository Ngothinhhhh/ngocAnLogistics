using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "tblOrder",
                type: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','AwaitingApproval','Completed','Cancelled','FailedDelivery')",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','Completed','Cancelled','FailedDelivery')",
                oldDefaultValue: "Pending")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "tblOrder",
                type: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','Completed','Cancelled','FailedDelivery')",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','AwaitingApproval','Completed','Cancelled','FailedDelivery')",
                oldDefaultValue: "Pending")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
