using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseEnquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "course_enquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CourseInterested = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "New"),
                    NextFollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedCounselorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConvertedCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_enquiry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_course_enquiry_candidate_ConvertedCandidateId",
                        column: x => x.ConvertedCandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_course_enquiry_staff_AssignedCounselorId",
                        column: x => x.AssignedCounselorId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enquiry_note",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enquiry_note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enquiry_note_course_enquiry_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "course_enquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enquiry_note_staff_AuthorStaffId",
                        column: x => x.AuthorStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_AssignedCounselorId",
                table: "course_enquiry",
                column: "AssignedCounselorId");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_ConvertedCandidateId",
                table: "course_enquiry",
                column: "ConvertedCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_NextFollowUpDate",
                table: "course_enquiry",
                column: "NextFollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_Phone",
                table: "course_enquiry",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_Status",
                table: "course_enquiry",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_enquiry_note_AuthorStaffId",
                table: "enquiry_note",
                column: "AuthorStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_enquiry_note_EnquiryId",
                table: "enquiry_note",
                column: "EnquiryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enquiry_note");

            migrationBuilder.DropTable(
                name: "course_enquiry");
        }
    }
}
