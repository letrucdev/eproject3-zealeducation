using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionInstallmentLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentPlanId",
                table: "payment_transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transaction_InstallmentPlanId",
                table: "payment_transaction",
                column: "InstallmentPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_transaction_installment_plan_InstallmentPlanId",
                table: "payment_transaction",
                column: "InstallmentPlanId",
                principalTable: "installment_plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_transaction_installment_plan_InstallmentPlanId",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "IX_payment_transaction_InstallmentPlanId",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "InstallmentPlanId",
                table: "payment_transaction");
        }
    }
}
