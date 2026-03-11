using SqlSugar;
using System.Data.Common;

namespace Nova.Shared.MultiTenancy;

/// <summary>
/// MySQL 数据库初始化辅助类
/// <para>确保目标连接指向的数据库在首次访问前已存在</para>
/// </summary>
public static class MySqlDatabaseCreator
{
    public static void EnsureCreated(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        if (!builder.TryGetValue("Database", out var databaseObj) &&
            !builder.TryGetValue("Initial Catalog", out databaseObj))
            return;

        var databaseName = databaseObj?.ToString();
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        builder.Remove("Database");
        builder.Remove("Initial Catalog");

        var adminDb = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.MySql,
            ConnectionString = builder.ConnectionString,
            IsAutoCloseConnection = true
        });

        var escapedDatabaseName = databaseName.Replace("`", "``", StringComparison.Ordinal);
        adminDb.Ado.ExecuteCommand(
            $"CREATE DATABASE IF NOT EXISTS `{escapedDatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }
}
