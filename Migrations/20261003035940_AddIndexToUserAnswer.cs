using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToUserAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuizHistoryId",
                table: "UserAnswers");

            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "QuizHistories",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuizHistoryId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "QuizHistoryId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuizHistoryId_QuestionId",
                table: "UserAnswers");

            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "QuizHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuizHistoryId",
                table: "UserAnswers",
                column: "QuizHistoryId");
        }
    }
}
