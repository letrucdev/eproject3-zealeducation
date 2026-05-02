using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedFieldsToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "feedback",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "feedback",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessedById",
                table: "feedback",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_is_processed",
                table: "feedback",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_ProcessedById",
                table: "feedback",
                column: "ProcessedById");

            migrationBuilder.AddForeignKey(
                name: "FK_feedback_user_account_ProcessedById",
                table: "feedback",
                column: "ProcessedById",
                principalTable: "user_account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feedback_user_account_ProcessedById",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "ix_feedback_is_processed",
                table: "feedback");

            migrationBuilder.DropIndex(
                name: "IX_feedback_ProcessedById",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "feedback");

            migrationBuilder.DropColumn(
                name: "ProcessedById",
                table: "feedback");
        }
    }
}
