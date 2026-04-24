using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchAndEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "batch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NeedsInstructor"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_batch_course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_batch_faculty_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "faculty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InchargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnrollmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PendingAssignment"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enrollment_batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_staff_InchargeId",
                        column: x => x.InchargeId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_batch_BatchCode",
                table: "batch",
                column: "BatchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_batch_CourseId",
                table: "batch",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_FacultyId",
                table: "batch",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_BatchId",
                table: "enrollment",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_CandidateId",
                table: "enrollment",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_CourseId",
                table: "enrollment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_InchargeId",
                table: "enrollment",
                column: "InchargeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollment");

            migrationBuilder.DropTable(
                name: "batch");
        }
    }
}
