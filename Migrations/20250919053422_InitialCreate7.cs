using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication3.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_tblOrder_OrderID",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_tblUser_UserID",
                table: "Activity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activity",
                table: "Activity");

            migrationBuilder.RenameTable(
                name: "Activity",
                newName: "tblActivity");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_UserID",
                table: "tblActivity",
                newName: "IX_tblActivity_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_OrderID",
                table: "tblActivity",
                newName: "IX_tblActivity_OrderID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblActivity",
                table: "tblActivity",
                column: "ActivityID");

            migrationBuilder.AddForeignKey(
                name: "FK_tblActivity_tblOrder_OrderID",
                table: "tblActivity",
                column: "OrderID",
                principalTable: "tblOrder",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblActivity_tblUser_UserID",
                table: "tblActivity",
                column: "UserID",
                principalTable: "tblUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblActivity_tblOrder_OrderID",
                table: "tblActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_tblActivity_tblUser_UserID",
                table: "tblActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tblActivity",
                table: "tblActivity");

            migrationBuilder.RenameTable(
                name: "tblActivity",
                newName: "Activity");

            migrationBuilder.RenameIndex(
                name: "IX_tblActivity_UserID",
                table: "Activity",
                newName: "IX_Activity_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_tblActivity_OrderID",
                table: "Activity",
                newName: "IX_Activity_OrderID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activity",
                table: "Activity",
                column: "ActivityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_tblOrder_OrderID",
                table: "Activity",
                column: "OrderID",
                principalTable: "tblOrder",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_tblUser_UserID",
                table: "Activity",
                column: "UserID",
                principalTable: "tblUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
