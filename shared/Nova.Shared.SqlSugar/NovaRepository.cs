using System.Linq.Expressions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using SqlSugar;

namespace Nova.Shared.SqlSugar;

/// <summary>
/// Nova 通用仓储 SqlSugar 实现
/// <para>封装 EntityBase 的标准 CRUD、分页、软删能力</para>
/// <para>各服务专用仓储继承此类，只需补充领域特有方法</para>
/// </summary>
public class NovaRepository<TEntity> : INovaRepository<TEntity>
    where TEntity : EntityBase, new()
{
    public ISqlSugarClient Db { get; }

    public ISugarQueryable<TEntity> Queryable => Db.Queryable<TEntity>();

    public NovaRepository(ISqlSugarClient db)
    {
        Db = db;
    }

    #region 查询

    /// <summary>根据主键 ID 查询单条记录</summary>
    public virtual async Task<TEntity?> GetByIdAsync(long id)
    {
        return await Db.Queryable<TEntity>().InSingleAsync(id);
    }

    /// <summary>根据条件查询第一条记录</summary>
    public virtual async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Queryable<TEntity>().FirstAsync(predicate);
    }

    /// <summary>判断是否存在满足条件的记录</summary>
    public virtual async Task<bool> IsAnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Queryable<TEntity>().AnyAsync(predicate);
    }

    /// <summary>统计满足条件的记录数</summary>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Queryable<TEntity>().CountAsync(predicate);
    }

    public virtual async Task<List<TEntity>> GetListAsync()
    {
        return await Db.Queryable<TEntity>().ToListAsync();
    }

    public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Queryable<TEntity>().Where(predicate).ToListAsync();
    }

    public virtual async Task<List<TEntity>> GetPageListAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageIndex,
        int pageSize,
        RefAsync<int> total)
    {
        return await Db.Queryable<TEntity>()
            .Where(predicate)
            .ToPageListAsync(pageIndex, pageSize, total);
    }

    public virtual async Task<List<TEntity>> GetPageListAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageIndex,
        int pageSize,
        RefAsync<int> total,
        Expression<Func<TEntity, object>>? orderBy = null,
        OrderByType orderByType = OrderByType.Asc)
    {
        var query = Db.Queryable<TEntity>().Where(predicate);

        if (orderBy != null)
            query = query.OrderBy(orderBy, orderByType);

        return await query.ToPageListAsync(pageIndex, pageSize, total);
    }

    #endregion

    #region 插入

    public virtual async Task<bool> InsertAsync(TEntity entity)
    {
        return await Db.Insertable(entity).ExecuteCommandAsync() > 0;
    }

    public virtual async Task<TEntity> InsertReturnEntityAsync(TEntity entity)
    {
        return await Db.Insertable(entity).ExecuteReturnEntityAsync();
    }

    public virtual async Task<bool> InsertRangeAsync(List<TEntity> entities)
    {
        return await Db.Insertable(entities).ExecuteCommandAsync() > 0;
    }

    #endregion

    #region 更新

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        entity.UpdateTime = DateTime.UtcNow;
        return await Db.Updateable(entity).ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> UpdateRangeAsync(List<TEntity> entities)
    {
        foreach (var entity in entities)
            entity.UpdateTime = DateTime.UtcNow;

        return await Db.Updateable(entities).ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> UpdateAsync(
        Expression<Func<TEntity, TEntity>> columns,
        Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Updateable<TEntity>()
            .SetColumns(columns)
            .Where(predicate)
            .ExecuteCommandAsync() > 0;
    }

    #endregion

    #region 删除

    public virtual async Task<bool> DeleteByIdAsync(long id)
    {
        return await Db.Deleteable<TEntity>().In(id).ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Deleteable<TEntity>().Where(predicate).ExecuteCommandAsync() > 0;
    }

    /// <summary>软删除（设置 IsDeleted = true）</summary>
    public virtual async Task<bool> SoftDeleteAsync(long id)
    {
        return await Db.Updateable<TEntity>()
            .SetColumns(e => e.IsDeleted == true)
            .SetColumns(e => e.UpdateTime == DateTime.UtcNow)
            .Where(e => e.Id == id)
            .ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> SoftDeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Db.Updateable<TEntity>()
            .SetColumns(e => e.IsDeleted == true)
            .SetColumns(e => e.UpdateTime == DateTime.UtcNow)
            .Where(predicate)
            .ExecuteCommandAsync() > 0;
    }

    #endregion
}
