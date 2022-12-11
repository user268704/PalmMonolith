#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Palm.Migrations.SessionData;

/// <inheritdoc />
public partial class SessionMigration1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Questions",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                Title = table.Column<string>("text", nullable: false),
                Description = table.Column<string>("text", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Questions", x => x.Id); });

        migrationBuilder.CreateTable(
            "SessionGroupInfo",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GroupName = table.Column<string>("text", nullable: false),
                TeacherId = table.Column<string>("text", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_SessionGroupInfo", x => x.Id); });

        migrationBuilder.CreateTable(
            "Answer",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Text = table.Column<string>("text", nullable: false),
                IsCorrect = table.Column<bool>("boolean", nullable: false),
                QuestionId = table.Column<Guid>("uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Answer", x => x.Id);
                table.ForeignKey(
                    "FK_Answer_Questions_QuestionId",
                    x => x.QuestionId,
                    "Questions",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "Sessions",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                HostId = table.Column<Guid>("uuid", nullable: false),
                ShortId = table.Column<string>("text", nullable: false),
                Title = table.Column<string>("text", nullable: false),
                GroupInfoId = table.Column<int>("integer", nullable: false),
                Students = table.Column<List<string>>("text[]", nullable: false),
                Questions = table.Column<List<string>>("text[]", nullable: false),
                StartDate = table.Column<DateTime>("timestamp with time zone", nullable: false),
                EndDate = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
                table.ForeignKey(
                    "FK_Sessions_SessionGroupInfo_GroupInfoId",
                    x => x.GroupInfoId,
                    "SessionGroupInfo",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Take",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                StudentId = table.Column<string>("text", nullable: false),
                TimeStart = table.Column<TimeOnly>("time without time zone", nullable: false),
                TimeCompleted = table.Column<TimeOnly>("time without time zone", nullable: false),
                SessionId = table.Column<Guid>("uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Take", x => x.Id);
                table.ForeignKey(
                    "FK_Take_Sessions_SessionId",
                    x => x.SessionId,
                    "Sessions",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "QuestionAnswer",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                QuestionId = table.Column<string>("text", nullable: false),
                AnswerId = table.Column<int>("integer", nullable: false),
                TakeId = table.Column<Guid>("uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuestionAnswer", x => x.Id);
                table.ForeignKey(
                    "FK_QuestionAnswer_Take_TakeId",
                    x => x.TakeId,
                    "Take",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_Answer_QuestionId",
            "Answer",
            "QuestionId");

        migrationBuilder.CreateIndex(
            "IX_QuestionAnswer_TakeId",
            "QuestionAnswer",
            "TakeId");

        migrationBuilder.CreateIndex(
            "IX_Sessions_GroupInfoId",
            "Sessions",
            "GroupInfoId");

        migrationBuilder.CreateIndex(
            "IX_Take_SessionId",
            "Take",
            "SessionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Answer");

        migrationBuilder.DropTable(
            "QuestionAnswer");

        migrationBuilder.DropTable(
            "Questions");

        migrationBuilder.DropTable(
            "Take");

        migrationBuilder.DropTable(
            "Sessions");

        migrationBuilder.DropTable(
            "SessionGroupInfo");
    }
}