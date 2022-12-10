#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Palm.Migrations.SessionData
{
    /// <inheritdoc />
    public partial class SessionMigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTeacherConnected",
                table: "SessionGroupInfo",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTeacherConnected",
                table: "SessionGroupInfo");
        }
    }
}
