using Microsoft.EntityFrameworkCore.Migrations;

namespace Fileesharing.Migrations
{
    public partial class AddOriginalNameToUploads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Uploads",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Uploads");
        }
    }
}
