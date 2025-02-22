
using MFToolkit.App;
using Microsoft.EntityFrameworkCore;

namespace MFToolkit.EFCore.Services;
public class Repository<T> : DbContext, IRepository<T> where T : class
{

    public DbContext Context => MFApp.GetService<DbContext>() ?? throw new Exception("未注入DbContext");

    public DbSet<T> DbSet()
    {
        return Context.Set<T>();
    }

    public int Delete(T value)
    {
        DbSet().Remove(value);
        var count = Context.SaveChanges();

        return count;
    }

    public async Task<int> DeleteAsync(T value)
    {
        DbSet().Remove(value);

        return await Context.SaveChangesAsync();
    }

    public int DeleteId(object id)
    {
        var query = DbSet().Find(id);
        if (query == null) return 0;
        DbSet().Remove(query);
        var count = Context.SaveChanges();
        return count;
    }

    public async Task<int> DeleteIdAsync(object id)
    {
        var query = DbSet().Find(id);
        if (query == null) return 0;
        DbSet().Remove(query);
        return await Context.SaveChangesAsync();
    }

    public T? FindIdValue(object id)
    {
        return DbSet().Find(id);
    }

    public async Task<T?> FindIdValueAsync(object id)
    {
        return await DbSet().FindAsync(id);
    }

    public int Insert(T value)
    {
        DbSet().Add(value);
        return Context.SaveChanges();
    }

    public async Task<int> InsertAsync(T value)
    {
        await DbSet().AddAsync(value);
        return await Context.SaveChangesAsync();
    }

    public IQueryable<T> Queryable()
    {
        return DbSet().AsQueryable();
    }

    public int Update(T value)
    {
        DbSet().Update(value);
        return Context.SaveChanges();
    }

    public async Task<int> UpdateAsync(T value)
    {
        DbSet().Update(value);
        return await Context.SaveChangesAsync();
    }
}
public class model
{
    public int Id { get; set; }
}
public class V
{
    private readonly IRepository<model> repository;
    public V(IRepository<model> repository)
    {
        this.repository = repository;
    }
    public async Task A()
    {
      await  repository.Queryable().Where(q => q.Id == 0).ExecuteDeleteAsync();
    }
}