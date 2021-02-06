using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RhythmHelper.Data.Migrations
{
    public partial class _4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "VideoLengthMax",
                table: "Guilds",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "VideoLengthMin",
                table: "Guilds",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoLengthMax",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "VideoLengthMin",
                table: "Guilds");
        }
    }
}
