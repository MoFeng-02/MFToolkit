using SqlSugar;

namespace MFToolkit.SqlSugarCore.Extensions.Repository;

public class Repository<T> : SimpleClient<T>, IRepository<T> where T : class, new()
{
    public Repository(ISqlSugarClient db)
    {
        base.Context = Db = db;
    }
    public ISqlSugarClient Db { get; }
}
