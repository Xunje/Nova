namespace Nova.Shared.Hosting.Results;

/// <summary>
/// 统一API响应结果包装类
/// <para>所有API接口应使用此类包装返回数据，保持响应格式一致</para>
/// </summary>
/// <typeparam name="T">返回数据类型</typeparam>
public class ApiResult<T>
{
    /// <summary>
    /// 状态码（200表示成功，其他表示失败）
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 是否成功（Code == 200）
    /// </summary>
    public bool Success => Code == 200;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="data">返回数据</param>
    /// <param name="message">成功消息</param>
    /// <returns>成功的API响应</returns>
    public static ApiResult<T> Ok(T data, string message = "success")
    {
        return new ApiResult<T> { Code = 200, Message = message, Data = data };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="code">错误码（默认500）</param>
    /// <returns>失败的API响应</returns>
    public static ApiResult<T> Fail(string message, int code = 500)
    {
        return new ApiResult<T> { Code = code, Message = message };
    }
}
