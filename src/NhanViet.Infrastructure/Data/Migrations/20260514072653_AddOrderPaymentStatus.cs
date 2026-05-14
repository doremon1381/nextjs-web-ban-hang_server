using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhanViet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                schema: "public",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'Unpaid'");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_PaymentStatus",
                schema: "public",
                table: "Orders",
                columns: new[] { "Status", "PaymentStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_PaymentStatus",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                schema: "public",
                table: "Orders");
        }
    }
}
