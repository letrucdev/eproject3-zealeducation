using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetFacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feedback_batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_feedback_candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_feedback_faculty_TargetFacultyId",
                        column: x => x.TargetFacultyId,
                        principalTable: "faculty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feedback_BatchId",
                table: "feedback",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_TargetFacultyId",
                table: "feedback",
                column: "TargetFacultyId");

            migrationBuilder.CreateIndex(
                name: "ix_feedback_unique_no_target",
                table: "feedback",
                columns: new[] { "CandidateId", "BatchId", "Type" },
                unique: true,
                filter: "[TargetFacultyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_feedback_unique_target",
                table: "feedback",
                columns: new[] { "CandidateId", "BatchId", "Type", "TargetFacultyId" },
                unique: true,
                filter: "[TargetFacultyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback");
        }
    }
}
