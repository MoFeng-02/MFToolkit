using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Repository;

/// <summary>
/// 仓储提供模型
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepository<T> : ISimpleClient<T> where T : class, new()
{
}
