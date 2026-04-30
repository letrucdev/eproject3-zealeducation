using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "certificate_application",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CertificateFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificate_application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_certificate_application_enrollment_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "enrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_certificate_application_staff_ApprovedByStaffId",
                        column: x => x.ApprovedByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_certificate_application_ApprovedByStaffId",
                table: "certificate_application",
                column: "ApprovedByStaffId");

            migrationBuilder.CreateIndex(
                name: "ix_certificate_application_certificate_number",
                table: "certificate_application",
                column: "CertificateNumber",
                unique: true,
                filter: "[CertificateNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_certificate_application_enrollment",
                table: "certificate_application",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_certificate_application_status",
                table: "certificate_application",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificate_application");
        }
    }
}
