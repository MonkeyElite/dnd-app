using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DndApp.Inventory.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventorySetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.StorageLocationId);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLots",
                columns: table => new
                {
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    UnitCostMinor = table.Column<long>(type: "bigint", nullable: false),
                    AcquiredWorldDay = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLots", x => x.LotId);
                    table.CheckConstraint("CK_InventoryLots_QuantityOnHand_NonNegative", "\"QuantityOnHand\" >= 0");
                    table.CheckConstraint("CK_InventoryLots_UnitCostMinor_NonNegative", "\"UnitCostMinor\" >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryLots_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "StorageLocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    AdjustmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeltaQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorldDay = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.AdjustmentId);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_InventoryLots_LotId",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "StorageLocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_CampaignId",
                table: "InventoryAdjustments",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_CampaignId_WorldDay_CreatedAt",
                table: "InventoryAdjustments",
                columns: new[] { "CampaignId", "WorldDay", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_ItemId",
                table: "InventoryAdjustments",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_LotId",
                table: "InventoryAdjustments",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_StorageLocationId",
                table: "InventoryAdjustments",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_CampaignId",
                table: "InventoryLots",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_CampaignId_ItemId_StorageLocationId_AcquiredW~",
                table: "InventoryLots",
                columns: new[] { "CampaignId", "ItemId", "StorageLocationId", "AcquiredWorldDay", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_ItemId",
                table: "InventoryLots",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_StorageLocationId",
                table: "InventoryLots",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_CampaignId",
                table: "StorageLocations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_CampaignId_Name",
                table: "StorageLocations",
                columns: new[] { "CampaignId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_PlaceId",
                table: "StorageLocations",
                column: "PlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropTable(
                name: "InventoryLots");

            migrationBuilder.DropTable(
                name: "StorageLocations");
        }
    }
}
