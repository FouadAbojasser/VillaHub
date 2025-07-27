using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsInvoiceEmailed",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvoiceEmailed",
                table: "Bookings");
        }
    }
}
