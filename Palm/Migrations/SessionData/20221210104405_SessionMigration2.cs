#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Palm.Migrations.SessionData;

/// <inheritdoc />
public partial class SessionMigration2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            "ConnectionId",
            "Take",
            "text",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            "ConnectionId",
            "Take");
    }
}