using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agriactifs");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exploitations",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Province = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    TotalAreaHa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductionType = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exploitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDefinitions",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PropertyName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_PermissionDefinitions_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ThemeDefinitions",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CssVariables = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "agriactifs",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "agriactifs",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "agriactifs",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Company = table.Column<string>(type: "text", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "text", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExploitationUsers",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExploitationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExploitationUsers_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fournisseurs_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parcelles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AreaHa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SoilType = table.Column<string>(type: "text", nullable: true),
                    HasIrrigation = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcelles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcelles_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockArticles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LotNumber = table.Column<string>(type: "text", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockArticles_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RequiredPermissionCode = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinitions_PermissionDefinitions_RequiredPermissionC~",
                        column: x => x.RequiredPermissionCode,
                        principalSchema: "agriactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissionGrants",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    PermissionDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionGrants_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "agriactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissionGrants_PermissionDefinitions_PermissionDefini~",
                        column: x => x.PermissionDefinitionId,
                        principalSchema: "agriactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecuredEndpoints",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Area = table.Column<string>(type: "text", nullable: true),
                    Controller = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: true),
                    PermissionDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuredEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecuredEndpoints_PermissionDefinitions_PermissionDefinition~",
                        column: x => x.PermissionDefinitionId,
                        principalSchema: "agriactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    Tagline = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    ActiveThemeId = table.Column<int>(type: "integer", nullable: false),
                    DefaultCulture = table.Column<string>(type: "text", nullable: false),
                    SmtpHost = table.Column<string>(type: "text", nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    SmtpUser = table.Column<string>(type: "text", nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    RequireConfirmedEmail = table.Column<bool>(type: "boolean", nullable: false),
                    RequireTwoFactor = table.Column<bool>(type: "boolean", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxFailedAccessAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_ThemeDefinitions_ActiveThemeId",
                        column: x => x.ActiveThemeId,
                        principalSchema: "agriactifs",
                        principalTable: "ThemeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActifsAgricoles",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    InternalCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcquisitionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UsefulLifeYears = table.Column<int>(type: "integer", nullable: true),
                    ResidualValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ParcelleId = table.Column<int>(type: "integer", nullable: true),
                    LocationNote = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActifsAgricoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActifsAgricoles_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActifsAgricoles_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Assolements",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParcelleId = table.Column<int>(type: "integer", nullable: false),
                    Season = table.Column<string>(type: "text", nullable: false),
                    Culture = table.Column<string>(type: "text", nullable: false),
                    YieldPerHa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assolements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assolements_Parcelles_ParcelleId",
                        column: x => x.ParcelleId,
                        principalSchema: "agriactifs",
                        principalTable: "Parcelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMouvements",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockArticleId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    MovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMouvements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMouvements_StockArticles_StockArticleId",
                        column: x => x.StockArticleId,
                        principalSchema: "agriactifs",
                        principalTable: "StockArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                schema: "agriactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    ActifAgricoleId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LaborCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PartsCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Report = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interventions_ActifsAgricoles_ActifAgricoleId",
                        column: x => x.ActifAgricoleId,
                        principalSchema: "agriactifs",
                        principalTable: "ActifsAgricoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Interventions_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "agriactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "agriactifs",
                table: "ThemeDefinitions",
                columns: new[] { "Id", "Code", "CreatedAt", "CssVariables", "Description", "IsActive", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, "default", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#1e40af\",\"--gise-primary-dark\": \"#1e3a8a\",\"--gise-accent\": \"#0ea5e9\",\"--gise-accent-soft\": \"#e0f2fe\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#0f172a\",\"--gise-sidebar-hover\": \"#1e293b\",\"--gise-sidebar-active\": \"#2563eb\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f1f5f9\",\"--gise-border\": \"#e2e8f0\",\"--gise-text\": \"#0f172a\",\"--gise-text-muted\": \"#64748b\"}", "Palette bleue d'origine", true, true, "GISEBS Default" },
                    { 2, "corporate", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#374151\",\"--gise-primary-dark\": \"#1f2937\",\"--gise-accent\": \"#6b7280\",\"--gise-accent-soft\": \"#f3f4f6\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#111827\",\"--gise-sidebar-hover\": \"#1f2937\",\"--gise-sidebar-active\": \"#4b5563\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f9fafb\",\"--gise-border\": \"#e5e7eb\",\"--gise-text\": \"#111827\",\"--gise-text-muted\": \"#6b7280\"}", "Tons neutres professionnels", true, true, "Corporate" },
                    { 3, "ocean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#0d9488\",\"--gise-primary-dark\": \"#0f766e\",\"--gise-accent\": \"#06b6d4\",\"--gise-accent-soft\": \"#cffafe\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#134e4a\",\"--gise-sidebar-hover\": \"#115e59\",\"--gise-sidebar-active\": \"#0d9488\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f0fdfa\",\"--gise-border\": \"#ccfbf1\",\"--gise-text\": \"#134e4a\",\"--gise-text-muted\": \"#5eead4\"}", "Bleu-vert moderne", true, true, "Ocean" }
                });

            migrationBuilder.InsertData(
                schema: "agriactifs",
                table: "SystemSettings",
                columns: new[] { "Id", "ActiveThemeId", "AppName", "DefaultCulture", "LockoutMinutes", "LogoUrl", "MaxFailedAccessAttempts", "RequireConfirmedEmail", "RequireTwoFactor", "SessionTimeoutMinutes", "SmtpHost", "SmtpPort", "SmtpUseSsl", "SmtpUser", "Tagline", "UpdatedAt" },
                values: new object[] { 1, 1, "AgriActifs", "fr-FR", 15, null, 5, true, false, 30, null, 587, true, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_ActifsAgricoles_ExploitationId_InternalCode",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                columns: new[] { "ExploitationId", "InternalCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActifsAgricoles_ParcelleId",
                schema: "agriactifs",
                table: "ActifsAgricoles",
                column: "ParcelleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "agriactifs",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "agriactifs",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "agriactifs",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "agriactifs",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "agriactifs",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "agriactifs",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "agriactifs",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assolements_ParcelleId",
                schema: "agriactifs",
                table: "Assolements",
                column: "ParcelleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                schema: "agriactifs",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "agriactifs",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExploitationUsers_ExploitationId_UserId",
                schema: "agriactifs",
                table: "ExploitationUsers",
                columns: new[] { "ExploitationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseurs_ExploitationId",
                schema: "agriactifs",
                table: "Fournisseurs",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ActifAgricoleId",
                schema: "agriactifs",
                table: "Interventions",
                column: "ActifAgricoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ExploitationId",
                schema: "agriactifs",
                table: "Interventions",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcelles_ExploitationId_Code",
                schema: "agriactifs",
                table: "Parcelles",
                columns: new[] { "ExploitationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDefinitions_Code",
                schema: "agriactifs",
                table: "PermissionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDefinitions_Resource_Action_PropertyName",
                schema: "agriactifs",
                table: "PermissionDefinitions",
                columns: new[] { "Resource", "Action", "PropertyName" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_Code",
                schema: "agriactifs",
                table: "ReportDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_RequiredPermissionCode",
                schema: "agriactifs",
                table: "ReportDefinitions",
                column: "RequiredPermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionGrants_PermissionDefinitionId",
                schema: "agriactifs",
                table: "RolePermissionGrants",
                column: "PermissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionGrants_RoleId_PermissionDefinitionId",
                schema: "agriactifs",
                table: "RolePermissionGrants",
                columns: new[] { "RoleId", "PermissionDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecuredEndpoints_Area_Controller_Action_HttpMethod",
                schema: "agriactifs",
                table: "SecuredEndpoints",
                columns: new[] { "Area", "Controller", "Action", "HttpMethod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecuredEndpoints_PermissionDefinitionId",
                schema: "agriactifs",
                table: "SecuredEndpoints",
                column: "PermissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockArticles_ExploitationId_Sku",
                schema: "agriactifs",
                table: "StockArticles",
                columns: new[] { "ExploitationId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMouvements_StockArticleId",
                schema: "agriactifs",
                table: "StockMouvements",
                column: "StockArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_ActiveThemeId",
                schema: "agriactifs",
                table: "SystemSettings",
                column: "ActiveThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeDefinitions_Code",
                schema: "agriactifs",
                table: "ThemeDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                schema: "agriactifs",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "Assolements",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "ExploitationUsers",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "Fournisseurs",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "Interventions",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "ReportDefinitions",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "RolePermissionGrants",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "SecuredEndpoints",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "StockMouvements",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "SystemSettings",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "ActifsAgricoles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "PermissionDefinitions",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "StockArticles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "ThemeDefinitions",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "Parcelles",
                schema: "agriactifs");

            migrationBuilder.DropTable(
                name: "Exploitations",
                schema: "agriactifs");
        }
    }
}
