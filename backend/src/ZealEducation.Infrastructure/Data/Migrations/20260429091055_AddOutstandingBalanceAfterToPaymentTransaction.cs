using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutstandingBalanceAfterToPaymentTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingBalanceAfter",
                table: "payment_transaction",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            // Backfill snapshot for existing transactions: TotalFee minus cumulative amount paid
            // up to and including this transaction, ordered chronologically (PaymentDate, then Id
            // as tiebreaker for transactions with identical timestamps).
            migrationBuilder.Sql(@"
WITH cum AS (
    SELECT
        t.Id,
        f.TotalFee - SUM(t.Amount) OVER (
            PARTITION BY t.FeeId
            ORDER BY t.PaymentDate, t.Id
            ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
        ) AS NewOutstanding
    FROM payment_transaction t
    INNER JOIN fee_structure f ON f.Id = t.FeeId
)
UPDATE pt
SET OutstandingBalanceAfter = cum.NewOutstanding
FROM payment_transaction pt
INNER JOIN cum ON cum.Id = pt.Id;
");

            // Invalidate previously cached receipt PDFs so they are regenerated from the
            // freshly-snapshotted OutstandingBalanceAfter. The old R2 objects are left in place
            // (orphaned) — clean them up separately if needed.
            migrationBuilder.Sql("UPDATE payment_transaction SET ReceiptFilePath = NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutstandingBalanceAfter",
                table: "payment_transaction");
        }
    }
}
