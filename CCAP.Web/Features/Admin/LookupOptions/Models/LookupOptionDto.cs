namespace CCAP.Web.Features.Admin.LookupOptions.Models;
public sealed class LookupOptionDto
{
    public Guid LookupOptionId { get; set; }
    public string LookupType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
