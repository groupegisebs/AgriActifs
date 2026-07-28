using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase34CarteIoTTerrain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "GpsLat",
                schema: "agriactifs",
                table: "Parcelles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsLng",
                schema: "agriactifs",
                table: "Parcelles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MapCenterLat",
                schema: "agriactifs",
                table: "Exploitations",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MapCenterLng",
                schema: "agriactifs",
                table: "Exploitations",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CapteursIoT",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    LastValue = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    LastReadingAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlertMin = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    AlertMax = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    ActifAgricoleId = table.Column<int>(type: "integer", nullable: true),
                    ParcelleId = table.Column<int>(type: "integer", nullable: true),
                    GpsLat = table.Column<double>(type: "double precision", nullable: true),
                    GpsLng = table.Column<double>(type: "double precision", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapteursIoT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapteursIoT_ActifsAgricoles_ActifAgricoleId",
                        column: x => x.ActifAgricoleId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CapteursIoT_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapteursIoT_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CapteurLectures",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapteurIoTId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapteurLectures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapteurLectures_CapteursIoT_CapteurIoTId",
                        column: x => x.CapteurIoTId,
                        principalSchema: "agriactifs",
                        principalTable: "CapteursIoT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapteurLectures_CapteurIoTId",
                schema: "agriactifs",
                table: "CapteurLectures",
                column: "CapteurIoTId");

            migrationBuilder.CreateIndex(
                name: "IX_CapteursIoT_ActifAgricoleId",
                schema: "agriactifs",
                table: "CapteursIoT",
                column: "ActifAgricoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CapteursIoT_ExploitationId_Code",
                schema: "agriactifs",
                table: "CapteursIoT",
                columns: new[] { "ExploitationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapteursIoT_ParcelleId",
                schema: "agriactifs",
                table: "CapteursIoT",
                column: "ParcelleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapteurLectures",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "CapteursIoT",
                schema: "agriactifs");

            migrationBuilder.DropColumn(
                name: "GpsLat",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "GpsLng",
                schema: "agriactifs",
                table: "Parcelles");

            migrationBuilder.DropColumn(
                name: "MapCenterLat",
                schema: "agriactifs",
                table: "Exploitations");

            migrationBuilder.DropColumn(
                name: "MapCenterLng",
                schema: "agriactifs",
                table: "Exploitations");
        }
    }
}
