﻿using MFToolkit.SqlSugarCore.Extensions.Configuration;
using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Context;

/// <summary>
/// 全称：SqlSugarAotDataBaseContext
/// </summary>
public class DbContext
{
    /// <summary>
    /// 全局配置，单个库配置还是多个库配置，如果设置了，那就局内配置不生效
    /// <para>true为单库配置，false为多库配置</para>
    /// </summary>
    public static bool? IsSingleConfig;

    /// <summary>
    /// 全局配置，如果设置了，那就局内配置不生效
    /// </summary>
    public static Action<SqlSugarClient>? ConfigAction;

    /// <summary>
    /// 创建SqlSugar实例
    /// </summary>
    /// <param name="isSingleConfig">单个库配置还是多个库配置</param>
    /// <param name="connectionConfig">独特单个配置操作</param>
    /// <param name="configAction">配置操作</param>
    /// <exception cref="Exception">数据库连接配置未初始化</exception>
    /// <returns></returns>
    public static SqlSugarClient CreateClient(bool? isSingleConfig = null, ConnectionConfig? connectionConfig = null, Action<SqlSugarClient>? configAction = null)
    {
        if (SqlSugarConfiguration.ConnectionConfig == null && (SqlSugarConfiguration.ConnectionConfigs == null ||
                                                                  SqlSugarConfiguration.ConnectionConfigs.Count ==
                                                                  0))
            throw new(
                "数据库连接配置未初始化，它应该如此初始化：在应用程序启动目录等前文件中调用 SqlSugarConfiguration.SetConnectionConfig 或 SqlSugarConfiguration.SetConnectionConfigs 方法来初始化它");
        if (IsSingleConfig != null && isSingleConfig == null) isSingleConfig = IsSingleConfig.Value;
        else isSingleConfig = true;
        if (ConfigAction != null) configAction ??= ConfigAction;
        var db = isSingleConfig.Value
            ? new SqlSugarClient(connectionConfig ?? SqlSugarConfiguration.ConnectionConfig, (action) => configAction?.Invoke(action))
            : new SqlSugarClient(SqlSugarConfiguration.ConnectionConfigs, (action) => configAction?.Invoke(action));
        return db;
    }
}