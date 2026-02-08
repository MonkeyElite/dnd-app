using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DndApp.Campaign.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCampaignSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.CampaignId);
                });

            migrationBuilder.CreateTable(
                name: "CalendarConfig",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekLength = table.Column<int>(type: "integer", nullable: false),
                    MonthsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarConfig", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_CalendarConfig_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyConfig",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MinorUnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MajorUnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DenominationsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyConfig", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_CurrencyConfig_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcCustomers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcCustomers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_NpcCustomers_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    PlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.PlaceId);
                    table.ForeignKey(
                        name: "FK_Places_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcCustomers_CampaignId",
                table: "NpcCustomers",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcCustomers_CampaignId_Name",
                table: "NpcCustomers",
                columns: new[] { "CampaignId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Places_CampaignId",
                table: "Places",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Places_CampaignId_Name",
                table: "Places",
                columns: new[] { "CampaignId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarConfig");

            migrationBuilder.DropTable(
                name: "CurrencyConfig");

            migrationBuilder.DropTable(
                name: "NpcCustomers");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
