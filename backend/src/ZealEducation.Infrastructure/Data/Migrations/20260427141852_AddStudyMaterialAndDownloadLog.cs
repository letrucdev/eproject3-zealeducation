using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyMaterialAndDownloadLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "study_material",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileSizeMb = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_material", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_material_course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_study_material_staff_UploadedByStaffId",
                        column: x => x.UploadedByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "material_download_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_download_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_material_download_log_candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_material_download_log_study_material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "study_material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_material_download_log_CandidateId",
                table: "material_download_log",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_material_download_log_MaterialId_DownloadedAt",
                table: "material_download_log",
                columns: new[] { "MaterialId", "DownloadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_study_material_CourseId",
                table: "study_material",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_study_material_CourseId_IsActive",
                table: "study_material",
                columns: new[] { "CourseId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_study_material_FilePath",
                table: "study_material",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_study_material_IsActive",
                table: "study_material",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_study_material_UploadedByStaffId",
                table: "study_material",
                column: "UploadedByStaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_download_log");

            migrationBuilder.DropTable(
                name: "study_material");
        }
    }
}
