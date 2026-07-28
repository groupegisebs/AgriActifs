using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultThemeToAgriPalette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "agriactifs",
                table: "ThemeDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CssVariables", "Description", "Name" },
                values: new object[] { "{\"--gise-primary\": \"#1b4d3e\",\"--gise-primary-dark\": \"#12352b\",\"--gise-accent\": \"#c4a35a\",\"--gise-accent-soft\": \"#f3ead3\",\"--gise-success\": \"#2f6f4e\",\"--gise-warning\": \"#b8860b\",\"--gise-danger\": \"#b42318\",\"--gise-sidebar\": \"#12352b\",\"--gise-sidebar-hover\": \"#1b4d3e\",\"--gise-sidebar-active\": \"#2a6350\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#eef3ef\",\"--gise-border\": \"#d5e0d8\",\"--gise-text\": \"#12352b\",\"--gise-text-muted\": \"#5c7268\"}", "Verts champs et or moisson", "AgriActifs" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "agriactifs",
                table: "ThemeDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CssVariables", "Description", "Name" },
                values: new object[] { "{\"--gise-primary\": \"#1e40af\",\"--gise-primary-dark\": \"#1e3a8a\",\"--gise-accent\": \"#0ea5e9\",\"--gise-accent-soft\": \"#e0f2fe\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#0f172a\",\"--gise-sidebar-hover\": \"#1e293b\",\"--gise-sidebar-active\": \"#2563eb\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f1f5f9\",\"--gise-border\": \"#e2e8f0\",\"--gise-text\": \"#0f172a\",\"--gise-text-muted\": \"#64748b\"}", "Palette bleue d'origine", "GISEBS Default" });
        }
    }
}
