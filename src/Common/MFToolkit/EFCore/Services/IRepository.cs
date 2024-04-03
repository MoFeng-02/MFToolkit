using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MFToolkit.EFCore.Services;
public interface IRepository<T> : ICurrentDbContext where T : class
{
    DbSet<T> DbSet();
    /// <summary>
    /// 获取全部但未执行查询
    /// </summary>
    /// <returns></returns>
    IQueryable<T> Queryable();
    /// <summary>
    /// ID查找值
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T? FindIdValue(object id);
    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int Insert(T value);
    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int Update(T value);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int Delete(T value);
    /// <summary>
    /// 根据ID删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    int DeleteId(object id);





    /// <summary>
    /// ID查找值
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T?> FindIdValueAsync(object id);
    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<int> InsertAsync(T value);
    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(T value);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(T value);
    /// <summary>
    /// 根据ID删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<int> DeleteIdAsync(object id);
}
