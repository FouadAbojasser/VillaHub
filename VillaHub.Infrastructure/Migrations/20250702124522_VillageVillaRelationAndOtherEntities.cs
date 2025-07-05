using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VillageVillaRelationAndOtherEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "Villas",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Villas",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "Villages",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Villages",
                newName: "Latitude");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFloors",
                table: "Villas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                table: "Villas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Villages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Villages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "entertainments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entertainments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    FoolrNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floors", x => x.FoolrNumber);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Villas_VillageId",
                table: "Villas",
                column: "VillageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Villas_Villages_VillageId",
                table: "Villas",
                column: "VillageId",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Villas_Villages_VillageId",
                table: "Villas");

            migrationBuilder.DropTable(
                name: "entertainments");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Villas_VillageId",
                table: "Villas");

            migrationBuilder.DropColumn(
                name: "NumberOfFloors",
                table: "Villas");

            migrationBuilder.DropColumn(
                name: "VillageId",
                table: "Villas");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Villages");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Villas",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Villas",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Villages",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Villages",
                newName: "latitude");
        }
    }
}
