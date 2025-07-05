using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelationBetweenVillaAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillaId",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Images_VillaId",
                table: "Images",
                column: "VillaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Villas_VillaId",
                table: "Images",
                column: "VillaId",
                principalTable: "Villas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Villas_VillaId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_VillaId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "VillaId",
                table: "Images");
        }
    }
}
