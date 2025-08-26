using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReplyToUserReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "RepliedAt",
                table: "Reviews",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reply",
                table: "Reviews",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReplyUserId",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Reply",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ReplyUserId",
                table: "Reviews");
        }
    }
}
