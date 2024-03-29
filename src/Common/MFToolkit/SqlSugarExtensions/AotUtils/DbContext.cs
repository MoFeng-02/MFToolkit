﻿using SqlSugar;

namespace MFToolkit.SqlSugarExtensions.AotUtils;

/// <summary>
/// 全称：SqlSugarAotDataBaseContext
/// </summary>
public class DbContext
{
    /// <summary>
    /// 全局配置，单个库配置还是多个库配置，如果设置了，那就局内配置不生效
    /// </summary>
    public static bool? IsSingleConfig;

    /// <summary>
    /// 全局配置，如果设置了，那就局内配置不生效
    /// </summary>
    public static Action<SqlSugarClient>? ConfigAction;

    /// <summary>
    /// 创建SqlSugarAot实例
    /// </summary>
    /// <param name="isSingleConfig">单个库配置还是多个库配置</param>
    /// <param name="configAction">配置操作</param>
    /// <exception cref="Exception">数据库连接配置未初始化</exception>
    /// <returns></returns>
    public static SqlSugarClient CreateClient(bool isSingleConfig = true, Action<SqlSugarClient>? configAction = null)
    {
        if (SqlSugarAotConfiguration.ConnectionConfig == null && (SqlSugarAotConfiguration.ConnectionConfigs == null ||
                                                                  SqlSugarAotConfiguration.ConnectionConfigs.Count ==
                                                                  0))
            throw new(
                "数据库连接配置未初始化，它应该如此初始化：在应用程序启动目录等前文件中调用 SqlSugarAotConfiguration.SetConnectionConfig 或 SqlSugarAotConfiguration.SetConnectionConfigs 方法来初始化它");
        if (IsSingleConfig != null && !isSingleConfig) isSingleConfig = IsSingleConfig.Value;
        if (ConfigAction != null) configAction ??= ConfigAction;
        var db = isSingleConfig
            ? new SqlSugarClient(SqlSugarAotConfiguration.ConnectionConfig, (action) => configAction?.Invoke(action))
            : new SqlSugarClient(SqlSugarAotConfiguration.ConnectionConfigs, (action) => configAction?.Invoke(action));
        return db;
    }
}