using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_log_user_account_UserId",
                        column: x => x.UserId,
                        principalTable: "user_account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Action",
                table: "audit_log",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_ChangedAt",
                table: "audit_log",
                column: "ChangedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_TableName_RecordId",
                table: "audit_log",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_UserId",
                table: "audit_log",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");
        }
    }
}
