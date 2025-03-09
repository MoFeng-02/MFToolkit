using System.Reflection;
using MFToolkit.SqlSugarCore.Extensions.Context;
using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Configuration;

public class SqlSugarConfiguration
{
    /// <summary>
    /// 单个连接配置
    /// </summary>
    public static ConnectionConfig? ConnectionConfig { get; private set; }

    /// <summary>
    /// 多个连接配置
    /// </summary>
    public static List<ConnectionConfig>? ConnectionConfigs { get; private set; }

    /// <summary>
    /// 设置单个连接库配置
    /// </summary>
    /// <param name="config"></param>
    public static void SetConnectionConfig(ConnectionConfig config) => ConnectionConfig = config;


    /// <summary>
    /// 设置连接库配置
    /// </summary>
    /// <param name="configs"></param>
    public static void SetConnectionConfigs(List<ConnectionConfig> configs) => ConnectionConfigs = configs;

    private static readonly ConfigureExternalServices DefaultConfigureExternalServices = new()
    {
        EntityService = (property, column) =>
        {
            try
            {
                //高版C#写法 支持string?和string Avalonia Android中不支持，不适用
                if (column.IsPrimarykey == false && new NullabilityInfoContext()
                        .Create(property).WriteState is NullabilityState.Nullable)
                {
                    column.IsNullable = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    };

    private static ConfigureExternalServices? _configureExternalServices;

    /// <summary>
    /// 默认配置 ConfigureExternalServices
    /// </summary>
    public static ConfigureExternalServices ConfigureExternalServices
    {
        get => _configureExternalServices ?? DefaultConfigureExternalServices;
        set => _configureExternalServices = value;
    }
    /// <summary>
    /// 初始化设计表 Code First模式（非Aot模式）
    /// <para>建库：如果不存在创建数据库存在不会重复创建</para>
    /// <para>注意：Oracle和个别国产库需不支持该方法，需要手动建库</para>
    /// </summary>
    public static void CreateDataBase(params Type[] entityTypes)
    {
        StaticConfig.EnableAot = false;
        using var client = DbContext.CreateClient();

        //var diffString = client.CodeFirst.GetDifferenceTables(entityTypes).ToDiffString();
        // 设置一下其他数据库配置
        client.CurrentConnectionConfig.ConfigureExternalServices = ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        client.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库
        // 建表
        client.CodeFirst.InitTables(entityTypes);
    }
    /// <summary>
    /// 初始化设计表 Code First模式（非Aot模式）
    /// <para>建库：如果不存在创建数据库存在不会重复创建</para>
    /// <para>注意：Oracle和个别国产库需不支持该方法，需要手动建库</para>
    /// </summary>
    public static void CreateDataBase(object? configId = null, params Type[] entityTypes)
    {
        StaticConfig.EnableAot = false;
        using var aotClient = DbContext.CreateClient();
        if (configId != null) aotClient.GetConnection(configId);
        // 设置一下其他数据库配置
        aotClient.CurrentConnectionConfig.ConfigureExternalServices = ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        aotClient.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库
        // 建表
        aotClient.CodeFirst.InitTables(entityTypes);
    }
    /// <summary>
    /// 初始化设计表 Code First模式（Aot模式）
    /// <para>建库：如果不存在创建数据库存在不会重复创建</para>
    /// <para>注意：Oracle和个别国产库需不支持该方法，需要手动建库</para>
    /// </summary>
    public static void CreateDataAotBase(params Type[] entityTypes)
    {
        StaticConfig.EnableAot = true;
        using var aotClient = DbContext.CreateClient();
        // 设置一下其他数据库配置
        aotClient.CurrentConnectionConfig.ConfigureExternalServices = ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        aotClient.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库
        // 建表
        aotClient.CodeFirst.InitTables(entityTypes);
    }
    /// <summary>
    /// 初始化设计表 Code First模式（Aot模式）
    /// <para>建库：如果不存在创建数据库存在不会重复创建</para>
    /// <para>注意：Oracle和个别国产库需不支持该方法，需要手动建库</para>
    /// </summary>
    public static void CreateDataAotBase(object? configId = null, params Type[] entityTypes)
    {
        StaticConfig.EnableAot = true;
        using var aotClient = DbContext.CreateClient();
        if (configId != null) aotClient.GetConnection(configId);
        // 设置一下其他数据库配置
        aotClient.CurrentConnectionConfig.ConfigureExternalServices = ConfigureExternalServices;
        //建库：如果不存在创建数据库存在不会重复创建 
        aotClient.DbMaintenance.CreateDatabase(); // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库
        // 建表
        aotClient.CodeFirst.InitTables(entityTypes);
    }
}