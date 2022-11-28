using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fileesharing.Migrations
{
    public partial class AddUploadDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "Uploads",
                nullable: false,
                defaultValue:"getDate()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Uploads");
        }
    }
}
