using System.ComponentModel.DataAnnotations;
namespace CCAP.Web.Features.Admin.LookupOptions.Models;
public sealed class LookupOptionEditModel
{
    public Guid LookupOptionId { get; set; }
    [Required] public string LookupType { get; set; } = string.Empty;
    [Required] public string Code { get; set; } = string.Empty;
    [Required] public string DisplayName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
