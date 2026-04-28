using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentFeeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeeId",
                table: "enrollment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_FeeId",
                table: "enrollment",
                column: "FeeId",
                unique: true,
                filter: "[FeeId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_enrollment_fee_structure_FeeId",
                table: "enrollment",
                column: "FeeId",
                principalTable: "fee_structure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_enrollment_fee_structure_FeeId",
                table: "enrollment");

            migrationBuilder.DropIndex(
                name: "IX_enrollment_FeeId",
                table: "enrollment");

            migrationBuilder.DropColumn(
                name: "FeeId",
                table: "enrollment");
        }
    }
}
