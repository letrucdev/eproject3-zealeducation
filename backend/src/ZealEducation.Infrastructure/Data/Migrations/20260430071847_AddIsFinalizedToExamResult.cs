using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinalizedToExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinalized",
                table: "exam_result",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Backfill: existing exam results were treated as official under the old flow,
            // so mark them all as finalized to preserve the historical invariant.
            migrationBuilder.Sql("UPDATE exam_result SET IsFinalized = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinalized",
                table: "exam_result");
        }
    }
}
