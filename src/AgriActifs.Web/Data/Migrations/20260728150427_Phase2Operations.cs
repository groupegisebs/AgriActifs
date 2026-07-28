using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase2Operations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToName",
                schema: "agriactifs",
                table: "Interventions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priorite",
                schema: "agriactifs",
                table: "Interventions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                schema: "agriactifs",
                table: "Interventions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForValidationAt",
                schema: "agriactifs",
                table: "Interventions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                schema: "agriactifs",
                table: "Interventions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatedByUserId",
                schema: "agriactifs",
                table: "Interventions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Categorie",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractRef",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "agriactifs",
                table: "Fournisseurs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivitesAgricoles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParcelleId = table.Column<int>(type: "integer", nullable: true),
                    ActifAgricoleId = table.Column<int>(type: "integer", nullable: true),
                    AssignedTo = table.Column<string>(type: "text", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitesAgricoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitesAgricoles_ActifsAgricoles_ActifAgricoleId",
                        column: x => x.ActifAgricoleId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActivitesAgricoles_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitesAgricoles_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentsFerme",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    ActifAgricoleId = table.Column<int>(type: "integer", nullable: true),
                    ParcelleId = table.Column<int>(type: "integer", nullable: true),
                    FournisseurId = table.Column<int>(type: "integer", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsFerme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentsFerme_ActifsAgricoles_ActifAgricoleId",
                        column: x => x.ActifAgricoleId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentsFerme_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentsFerme_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalSchema: "agriactifs",
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentsFerme_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnergieReleves",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    ActifAgricoleId = table.Column<int>(type: "integer", nullable: true),
                    ReadingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kwh = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergieReleves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergieReleves_ActifsAgricoles_ActifAgricoleId",
                        column: x => x.ActifAgricoleId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EnergieReleves_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FermeNotifications",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LinkController = table.Column<string>(type: "text", nullable: true),
                    LinkAction = table.Column<string>(type: "text", nullable: true),
                    LinkId = table.Column<int>(type: "integer", nullable: true),
                    DedupeKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FermeNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FermeNotifications_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IrrigationSecteurs",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParcelleId = table.Column<int>(type: "integer", nullable: true),
                    PompeActifId = table.Column<int>(type: "integer", nullable: true),
                    ReservoirNote = table.Column<string>(type: "text", nullable: true),
                    DebitM3H = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PressionBar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrrigationSecteurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IrrigationSecteurs_ActifsAgricoles_PompeActifId",
                        column: x => x.PompeActifId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IrrigationSecteurs_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IrrigationSecteurs_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivitesAgricoles_ActifAgricoleId",
                schema: "agriactifs",
                table: "ActivitesAgricoles",
                column: "ActifAgricoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitesAgricoles_ExploitationId",
                schema: "agriactifs",
                table: "ActivitesAgricoles",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitesAgricoles_ParcelleId",
                schema: "agriactifs",
                table: "ActivitesAgricoles",
                column: "ParcelleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsFerme_ActifAgricoleId",
                schema: "agriactifs",
                table: "DocumentsFerme",
                column: "ActifAgricoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsFerme_ExploitationId",
                schema: "agriactifs",
                table: "DocumentsFerme",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsFerme_FournisseurId",
                schema: "agriactifs",
                table: "DocumentsFerme",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsFerme_ParcelleId",
                schema: "agriactifs",
                table: "DocumentsFerme",
                column: "ParcelleId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergieReleves_ActifAgricoleId",
                schema: "agriactifs",
                table: "EnergieReleves",
                column: "ActifAgricoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergieReleves_ExploitationId",
                schema: "agriactifs",
                table: "EnergieReleves",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_FermeNotifications_ExploitationId_DedupeKey",
                schema: "agriactifs",
                table: "FermeNotifications",
                columns: new[] { "ExploitationId", "DedupeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationSecteurs_ExploitationId_Code",
                schema: "agriactifs",
                table: "IrrigationSecteurs",
                columns: new[] { "ExploitationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationSecteurs_ParcelleId",
                schema: "agriactifs",
                table: "IrrigationSecteurs",
                column: "ParcelleId");

            migrationBuilder.CreateIndex(
                name: "IX_IrrigationSecteurs_PompeActifId",
                schema: "agriactifs",
                table: "IrrigationSecteurs",
                column: "PompeActifId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivitesAgricoles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "DocumentsFerme",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "EnergieReleves",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "FermeNotifications",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "IrrigationSecteurs",
                schema: "agriactifs");

            migrationBuilder.DropColumn(
                name: "AssignedToName",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "Priorite",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "SubmittedForValidationAt",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "ValidatedByUserId",
                schema: "agriactifs",
                table: "Interventions");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "agriactifs",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "Categorie",
                schema: "agriactifs",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                schema: "agriactifs",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "ContractRef",
                schema: "agriactifs",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "Rating",
                schema: "agriactifs",
                table: "Fournisseurs");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "agriactifs",
                table: "Fournisseurs");
        }
    }
}
