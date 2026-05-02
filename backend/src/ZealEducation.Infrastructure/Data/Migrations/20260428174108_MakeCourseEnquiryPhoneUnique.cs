using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeCourseEnquiryPhoneUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_enquiry_Phone",
                table: "course_enquiry");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_Phone",
                table: "course_enquiry",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_enquiry_Phone",
                table: "course_enquiry");

            migrationBuilder.CreateIndex(
                name: "IX_course_enquiry_Phone",
                table: "course_enquiry",
                column: "Phone");
        }
    }
}
