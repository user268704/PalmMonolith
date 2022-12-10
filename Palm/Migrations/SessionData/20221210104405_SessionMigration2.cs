#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Palm.Migrations.SessionData
{
    /// <inheritdoc />
    public partial class SessionMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Take",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Take");
        }
    }
}
