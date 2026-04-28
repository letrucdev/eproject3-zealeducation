using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZealEducation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ConditionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMaintenance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_asset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_system_asset_staff_ManagedBy",
                        column: x => x.ManagedBy,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_asset_AssetType",
                table: "system_asset",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_system_asset_ConditionStatus",
                table: "system_asset",
                column: "ConditionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_system_asset_ManagedBy",
                table: "system_asset",
                column: "ManagedBy");

            migrationBuilder.CreateIndex(
                name: "IX_system_asset_SerialNumber",
                table: "system_asset",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_asset");
        }
    }
}
