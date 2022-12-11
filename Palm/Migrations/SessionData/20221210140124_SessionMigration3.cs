#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Palm.Migrations.SessionData;

/// <inheritdoc />
public partial class SessionMigration3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            "IsTeacherConnected",
            "SessionGroupInfo",
            "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            "IsTeacherConnected",
            "SessionGroupInfo");
    }
}