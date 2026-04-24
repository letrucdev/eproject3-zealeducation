using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeStructureAndRelated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fee_structure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalFee = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    FeeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Tuition"),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(12,2)", nullable: false, computedColumnSql: "[TotalFee] - [AmountPaid]", stored: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid"),
                    PaymentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NotSet"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_structure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fee_structure_candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViolationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fine_candidate_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fine_fee_structure_FeeId",
                        column: x => x.FeeId,
                        principalTable: "fee_structure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fine_staff_IssuedByStaffId",
                        column: x => x.IssuedByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "installment_plan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Pending"),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_installment_plan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_installment_plan_fee_structure_FeeId",
                        column: x => x.FeeId,
                        principalTable: "fee_structure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessedByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_transaction_fee_structure_FeeId",
                        column: x => x.FeeId,
                        principalTable: "fee_structure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_transaction_staff_ProcessedByStaffId",
                        column: x => x.ProcessedByStaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fee_structure_CandidateId",
                table: "fee_structure",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_fine_CandidateId",
                table: "fine",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_fine_FeeId",
                table: "fine",
                column: "FeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fine_IssuedByStaffId",
                table: "fine",
                column: "IssuedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_installment_plan_FeeId_InstallmentNo",
                table: "installment_plan",
                columns: new[] { "FeeId", "InstallmentNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transaction_FeeId",
                table: "payment_transaction",
                column: "FeeId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transaction_ProcessedByStaffId",
                table: "payment_transaction",
                column: "ProcessedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transaction_ReceiptNumber",
                table: "payment_transaction",
                column: "ReceiptNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fine");

            migrationBuilder.DropTable(
                name: "installment_plan");

            migrationBuilder.DropTable(
                name: "payment_transaction");

            migrationBuilder.DropTable(
                name: "fee_structure");
        }
    }
}
