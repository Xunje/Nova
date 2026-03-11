using System.Linq.Expressions;
using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.Shared.SqlSugar.Abstractions;

/// <summary>
/// Nova 通用仓储接口
/// <para>提供基于 EntityBase 的标准 CRUD、分页、软删能力</para>
/// <para>领域特有查询应在各服务自己的仓储接口中声明</para>
/// </summary>
public interface INovaRepository<TEntity> where TEntity : EntityBase, new()
{
    ISqlSugarClient Db { get; }

    ISugarQueryable<TEntity> Queryable { get; }

    #region 查询

    Task<TEntity?> GetByIdAsync(long id);

    Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> predicate);

    Task<bool> IsAnyAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>> GetListAsync();

    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>> GetPageListAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageIndex,
        int pageSize,
        RefAsync<int> total);

    Task<List<TEntity>> GetPageListAsync(
        Expression<Func<TEntity, bool>> predicate,
        int pageIndex,
        int pageSize,
        RefAsync<int> total,
        Expression<Func<TEntity, object>>? orderBy = null,
        OrderByType orderByType = OrderByType.Asc);

    #endregion

    #region 插入

    Task<bool> InsertAsync(TEntity entity);

    Task<TEntity> InsertReturnEntityAsync(TEntity entity);

    Task<bool> InsertRangeAsync(List<TEntity> entities);

    #endregion

    #region 更新

    Task<bool> UpdateAsync(TEntity entity);

    Task<bool> UpdateRangeAsync(List<TEntity> entities);

    Task<bool> UpdateAsync(
        Expression<Func<TEntity, TEntity>> columns,
        Expression<Func<TEntity, bool>> predicate);

    #endregion

    #region 删除

    Task<bool> DeleteByIdAsync(long id);

    Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate);

    Task<bool> SoftDeleteAsync(long id);

    Task<bool> SoftDeleteAsync(Expression<Func<TEntity, bool>> predicate);

    #endregion
}
