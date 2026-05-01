using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallmentReminderLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "installment_reminder_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstallmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderType = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DaysOffset = table.Column<int>(type: "int", nullable: false),
                    SentForDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_installment_reminder_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_installment_reminder_log_installment_plan_InstallmentPlanId",
                        column: x => x.InstallmentPlanId,
                        principalTable: "installment_plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_installment_reminder_log_InstallmentPlanId_SentForDate_ReminderType",
                table: "installment_reminder_log",
                columns: new[] { "InstallmentPlanId", "SentForDate", "ReminderType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_installment_reminder_log_SentForDate",
                table: "installment_reminder_log",
                column: "SentForDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installment_reminder_log");
        }
    }
}
