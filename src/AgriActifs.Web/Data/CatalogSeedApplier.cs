using Microsoft.EntityFrameworkCore.Migrations;

namespace AgriActifs.Web.Data;

/// <summary>
/// Insère le catalogue initial en BD via migration (idempotent — ne duplique pas si déjà présent).
/// </summary>
public static class CatalogSeedApplier
{
    public static void Up(MigrationBuilder migrationBuilder, string schema = "agriactifs")
    {
        var permissions = Table(schema, "PermissionDefinitions");
        var endpoints = Table(schema, "SecuredEndpoints");
        var reports = Table(schema, "ReportDefinitions");

        foreach (var permission in CatalogSeedData.Permissions)
        {
            var propertyName = permission.PropertyName is null
                ? "NULL"
                : $"'{Sql(permission.PropertyName)}'";

            migrationBuilder.Sql($"""
                INSERT INTO {permissions} ("Id", "Code", "Resource", "Action", "PropertyName", "DisplayName", "Category", "IsSystem", "IsActive")
                SELECT {permission.Id}, '{Sql(permission.Code)}', '{Sql(permission.Resource)}', {(int)permission.Action}, {propertyName}, '{Sql(permission.DisplayName)}', '{Sql(permission.Category)}', TRUE, TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {permissions} WHERE "Code" = '{Sql(permission.Code)}');
                """);
        }

        foreach (var endpoint in CatalogSeedData.Endpoints)
        {
            var area = endpoint.Area is null ? "NULL" : $"'{Sql(endpoint.Area)}'";
            var httpMethod = endpoint.HttpMethod is null ? "NULL" : $"'{Sql(endpoint.HttpMethod)}'";
            var areaMatch = endpoint.Area is null
                ? "e.\"Area\" IS NULL"
                : $"e.\"Area\" = '{Sql(endpoint.Area)}'";
            var httpMatch = endpoint.HttpMethod is null
                ? "e.\"HttpMethod\" IS NULL"
                : $"e.\"HttpMethod\" = '{Sql(endpoint.HttpMethod)}'";

            migrationBuilder.Sql($"""
                INSERT INTO {endpoints} ("Area", "Controller", "Action", "HttpMethod", "PermissionDefinitionId", "IsActive")
                SELECT {area}, '{Sql(endpoint.Controller)}', '{Sql(endpoint.Action)}', {httpMethod}, p."Id", TRUE
                FROM {permissions} p
                WHERE p."Code" = '{Sql(endpoint.PermissionCode)}'
                AND NOT EXISTS (
                    SELECT 1 FROM {endpoints} e
                    WHERE {areaMatch}
                      AND e."Controller" = '{Sql(endpoint.Controller)}'
                      AND e."Action" = '{Sql(endpoint.Action)}'
                      AND {httpMatch});
                """);
        }

        foreach (var report in CatalogSeedData.Reports)
        {
            migrationBuilder.Sql($"""
                INSERT INTO {reports} ("Id", "Code", "Name", "Category", "RequiredPermissionCode", "IsActive")
                SELECT {report.Id}, '{Sql(report.Code)}', '{Sql(report.Name)}', '{Sql(report.Category)}', '{Sql(report.RequiredPermissionCode)}', TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {reports} WHERE "Code" = '{Sql(report.Code)}');
                """);
        }

        migrationBuilder.Sql($"""
            SELECT setval(pg_get_serial_sequence('{schema}."PermissionDefinitions"', 'Id'), COALESCE((SELECT MAX("Id") FROM {permissions}), 1));
            SELECT setval(pg_get_serial_sequence('{schema}."SecuredEndpoints"', 'Id'), COALESCE((SELECT MAX("Id") FROM {endpoints}), 1));
            SELECT setval(pg_get_serial_sequence('{schema}."ReportDefinitions"', 'Id'), COALESCE((SELECT MAX("Id") FROM {reports}), 1));
            """);
    }

    private static string Table(string schema, string table) => $"\"{Sql(schema)}\".\"{table}\"";

    private static string Sql(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
