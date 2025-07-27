using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInvoiceEmaildToBookingTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvoiceEmailed",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInvoiceEmailed",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
