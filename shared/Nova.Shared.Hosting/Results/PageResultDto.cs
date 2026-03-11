namespace Nova.Shared.Hosting.Results;

/// <summary>
/// 分页查询结果DTO
/// <para>用于包装分页列表数据，包含总数和当前页数据</para>
/// </summary>
/// <typeparam name="T">列表项数据类型</typeparam>
public class PageResultDto<T>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// 当前页数据列表
    /// </summary>
    public List<T> Items { get; set; }

    public PageResultDto(long total, List<T> items)
    {
        Total = total;
        Items = items;
    }
}
