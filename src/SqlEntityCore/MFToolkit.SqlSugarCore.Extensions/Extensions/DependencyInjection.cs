using MFToolkit.SqlSugarCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Extensions;

/// <summary>
/// 提供相关的依赖注入扩展
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 添加SqlSugarCore的相关服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="connect">连接字符串Key</param>
    public static IServiceCollection AddSqlSugarCore(this IServiceCollection services, IConfiguration configuration, string connect = "Default")
    {
        services.AddScoped<ISqlSugarClient>(sp =>
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = configuration.GetConnectionString(connect),
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            return db;
        });
        return services;
    }

    /// <summary>
    /// 添加SqlSugarCore的相关服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    /// <exception cref="Exception">未配置相关参数</exception>
    public static IServiceCollection AddSqlSugarCore(this IServiceCollection services, Action<SqlSugarClient> configAction)
    {
        services.AddScoped<ISqlSugarClient>(sp =>
        {
            SqlSugarClient db;
            if (SqlSugarConfiguration.ConnectionConfig != null)
                db = new SqlSugarClient(SqlSugarConfiguration.ConnectionConfig, configAction);
            else if (SqlSugarConfiguration.ConnectionConfigs != null && SqlSugarConfiguration.ConnectionConfigs.Count > 0)
                db = new SqlSugarClient(SqlSugarConfiguration.ConnectionConfigs, configAction);
            else
                throw new Exception("数据库连接配置未初始化，它应该如此初始化：在应用程序启动目录等前文件中调用 SqlSugarConfiguration.SetConnectionConfig 或 SqlSugarConfiguration.SetConnectionConfigs 方法来初始化它");
            return db;
        });
        return services;
    }

}
