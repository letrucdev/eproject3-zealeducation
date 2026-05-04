using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPenaltyAppliedToFeeStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyApplied",
                table: "fee_structure",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "OutstandingBalance",
                table: "fee_structure",
                type: "decimal(12,2)",
                nullable: false,
                computedColumnSql: "[TotalFee] + [PenaltyApplied] - [AmountPaid]",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComputedColumnSql: "[TotalFee] - [AmountPaid]",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyApplied",
                table: "fee_structure");

            migrationBuilder.AlterColumn<decimal>(
                name: "OutstandingBalance",
                table: "fee_structure",
                type: "decimal(12,2)",
                nullable: false,
                computedColumnSql: "[TotalFee] - [AmountPaid]",
                stored: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldComputedColumnSql: "[TotalFee] + [PenaltyApplied] - [AmountPaid]",
                oldStored: true);
        }
    }
}
