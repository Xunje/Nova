namespace Nova.SystemService.Core.Dtos.Dict;

public class DictionaryDataDto
{
    public long Id { get; set; }
    public string DictType { get; set; } = string.Empty;
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public string? CssClass { get; set; }
    public string? ListClass { get; set; }
    public bool IsDefault { get; set; }
    public bool Status { get; set; }
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}
