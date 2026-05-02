using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExaminationAndExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "examination",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxScore = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    PassScore = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    ScheduledById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_examination_batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_examination_staff_ScheduledById",
                        column: x => x.ScheduledById,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_result",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    GradedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOverridden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OverrideById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_result_enrollment_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "enrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_result_examination_ExamId",
                        column: x => x.ExamId,
                        principalTable: "examination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_result_staff_GradedById",
                        column: x => x.GradedById,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_result_staff_OverrideById",
                        column: x => x.OverrideById,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_EnrollmentId",
                table: "exam_result",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_ExamId_EnrollmentId",
                table: "exam_result",
                columns: new[] { "ExamId", "EnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_GradedById",
                table: "exam_result",
                column: "GradedById");

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_OverrideById",
                table: "exam_result",
                column: "OverrideById");

            migrationBuilder.CreateIndex(
                name: "IX_examination_BatchId_ExamName_ExamDate",
                table: "examination",
                columns: new[] { "BatchId", "ExamName", "ExamDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_examination_ScheduledById",
                table: "examination",
                column: "ScheduledById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_result");

            migrationBuilder.DropTable(
                name: "examination");
        }
    }
}
