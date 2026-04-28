using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAndLinkEnquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseInterested",
                table: "course_enquiry");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseInterestedId",
                table: "course_enquiry",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "course",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationWeeks = table.Column<int>(type: "int", nullable: false),
                    BaseFee = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_CourseInterestedId",
                table: "course_enquiry",
                column: "CourseInterestedId");

            migrationBuilder.CreateIndex(
                name: "IX_course_CourseName",
                table: "course",
                column: "CourseName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_course_enquiry_course_CourseInterestedId",
                table: "course_enquiry",
                column: "CourseInterestedId",
                principalTable: "course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_enquiry_course_CourseInterestedId",
                table: "course_enquiry");

            migrationBuilder.DropTable(
                name: "course");

            migrationBuilder.DropIndex(
                name: "IX_course_enquiry_CourseInterestedId",
                table: "course_enquiry");

            migrationBuilder.DropColumn(
                name: "CourseInterestedId",
                table: "course_enquiry");

            migrationBuilder.AddColumn<string>(
                name: "CourseInterested",
                table: "course_enquiry",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
