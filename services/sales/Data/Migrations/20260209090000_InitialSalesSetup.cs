using System;
using DndApp.Sales.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DndApp.Sales.Data.Migrations;

[DbContext(typeof(SalesDbContext))]
[Migration("20260209090000_InitialSalesSetup")]
public sealed class InitialSalesSetup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            columns: table => new
            {
                OutboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                PublishAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                LastError = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.OutboxMessageId);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrders",
            columns: table => new
            {
                SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                SoldWorldDay = table.Column<int>(type: "integer", nullable: false),
                SubtotalMinor = table.Column<long>(type: "bigint", nullable: false),
                DiscountTotalMinor = table.Column<long>(type: "bigint", nullable: false),
                TaxTotalMinor = table.Column<long>(type: "bigint", nullable: false),
                TotalMinor = table.Column<long>(type: "bigint", nullable: false),
                Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrders", x => x.SaleId);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrderLines",
            columns: table => new
            {
                SaleLineId = table.Column<Guid>(type: "uuid", nullable: false),
                SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                UnitSoldPriceMinor = table.Column<long>(type: "bigint", nullable: false),
                UnitTrueValueMinor = table.Column<long>(type: "bigint", nullable: true),
                DiscountMinor = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                LineSubtotalMinor = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderLines", x => x.SaleLineId);
                table.ForeignKey(
                    name: "FK_SalesOrderLines_SalesOrders_SaleId",
                    column: x => x.SaleId,
                    principalTable: "SalesOrders",
                    principalColumn: "SaleId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SalesPayments",
            columns: table => new
            {
                PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                DetailsJson = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesPayments", x => x.PaymentId);
                table.ForeignKey(
                    name: "FK_SalesPayments_SalesOrders_SaleId",
                    column: x => x.SaleId,
                    principalTable: "SalesOrders",
                    principalColumn: "SaleId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_PublishedAt",
            table: "OutboxMessages",
            column: "PublishedAt");

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_Type_OccurredAt",
            table: "OutboxMessages",
            columns: new[] { "Type", "OccurredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_ItemId",
            table: "SalesOrderLines",
            column: "ItemId");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_SaleId",
            table: "SalesOrderLines",
            column: "SaleId");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrders_CampaignId",
            table: "SalesOrders",
            column: "CampaignId");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrders_CampaignId_SoldWorldDay",
            table: "SalesOrders",
            columns: new[] { "CampaignId", "SoldWorldDay" });

        migrationBuilder.CreateIndex(
            name: "IX_SalesPayments_SaleId",
            table: "SalesPayments",
            column: "SaleId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OutboxMessages");

        migrationBuilder.DropTable(
            name: "SalesOrderLines");

        migrationBuilder.DropTable(
            name: "SalesPayments");

        migrationBuilder.DropTable(
            name: "SalesOrders");
    }
}
