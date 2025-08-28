using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations
{
    public partial class AddChatMessageDisplayName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ChatMessages",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ChatMessages");
        }
    }
}
