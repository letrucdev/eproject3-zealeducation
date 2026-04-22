using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffFacultyCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    JoinedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(GETUTCDATE() AS date)"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_user_account_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "candidate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RegisteredByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_staff_RegisteredByStaffId",
                        column: x => x.RegisteredByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_candidate_user_account_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "faculty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacultyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_faculty_staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_CandidateCode",
                table: "candidate",
                column: "CandidateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidate_RegisteredByStaffId",
                table: "candidate",
                column: "RegisteredByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_UserAccountId",
                table: "candidate",
                column: "UserAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faculty_FacultyCode",
                table: "faculty",
                column: "FacultyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faculty_StaffId",
                table: "faculty",
                column: "StaffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_UserAccountId",
                table: "staff",
                column: "UserAccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate");

            migrationBuilder.DropTable(
                name: "faculty");

            migrationBuilder.DropTable(
                name: "staff");
        }
    }
}
