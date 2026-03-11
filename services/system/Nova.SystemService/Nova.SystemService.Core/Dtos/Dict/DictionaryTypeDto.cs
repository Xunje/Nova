namespace Nova.SystemService.Core.Dtos.Dict;

public class DictionaryTypeDto
{
    public long Id { get; set; }
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public bool Status { get; set; }
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}
