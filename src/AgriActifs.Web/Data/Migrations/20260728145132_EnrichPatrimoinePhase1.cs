using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnrichPatrimoinePhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualYieldPerHa",
                schema: "agriactifs",
                table: "Parcelles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentCulture",
                schema: "agriactifs",
                table: "Parcelles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedYieldPerHa",
                schema: "agriactifs",
                table: "Parcelles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Etat",
                schema: "agriactifs",
                table: "Parcelles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HarvestDate",
                schema: "agriactifs",
                table: "Parcelles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlannedCulture",
                schema: "agriactifs",
                table: "Parcelles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousCulture",
                schema: "agriactifs",
                table: "Parcelles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleName",
                schema: "agriactifs",
                table: "Parcelles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SowingDate",
                schema: "agriactifs",
                table: "Parcelles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Building",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EngineHours",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "numeric(18,1)",
                precision: 18,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsLat",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsLng",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextServiceDate",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NextServiceHours",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "numeric(18,1)",
                precision: 18,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OdometerKm",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "numeric(18,1)",
                precision: 18,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrPayload",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserId",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InterventionPieces",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InterventionMaintenanceId = table.Column<int>(type: "integer", nullable: false),
                    StockArticleId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Deducted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionPieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterventionPieces_Interventions_InterventionMaintenanceId",
                        column: x => x.InterventionMaintenanceId,
                        principalSchema: "agriactifs",
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterventionPieces_StockArticles_StockArticleId",
                        column: x => x.StockArticleId,
                        principalSchema: "agriactifs",
                        principalTable: "StockArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMouvements_InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements",
                column: "InterventionMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActifsAgricoles_FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionPieces_InterventionMaintenanceId",
                schema: "agriactifs",
                table: "InterventionPieces",
                column: "InterventionMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionPieces_StockArticleId",
                schema: "agriactifs",
                table: "InterventionPieces",
                column: "StockArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActifsAgricoles_Fournisseurs_FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                column: "FournisseurId",
                principalSchema: "agriactifs",
                principalTable: "Fournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMouvements_Interventions_InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements",
                column: "InterventionMaintenanceId",
                principalSchema: "agriactifs",
                principalTable: "Interventions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActifsAgricoles_Fournisseurs_FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMouvements_Interventions_InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements");

            migrationBuilder.DropTable(
                name: "InterventionPieces",
                schema: "agriactifs");

            migrationBuilder.DropIndex(
                name: "IX_StockMouvements_InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements");

            migrationBuilder.DropIndex(
                name: "IX_ActifsAgricoles_FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "InterventionMaintenanceId",
                schema: "agriactifs",
                table: "StockMouvements");

            migrationBuilder.DropColumn(
                name: "ActualYieldPerHa",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "CurrentCulture",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "EstimatedYieldPerHa",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "Etat",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "HarvestDate",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "PlannedCulture",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "PreviousCulture",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "ResponsibleName",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "SowingDate",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "Building",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "EngineHours",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "FournisseurId",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "GpsLat",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "GpsLng",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "NextServiceDate",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "NextServiceHours",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "OdometerKm",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "QrPayload",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                schema: "agriactifs",
                table: "ActifsAgricoles");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                schema: "agriactifs",
                table: "ActifsAgricoles");
        }
    }
}
