namespace CCAP.Application.Features.Admin.LookupOptions;

public sealed record LookupOptionDto(
    Guid LookupOptionId,
    string LookupType,
    string Code,
    string DisplayName,
    int SortOrder,
    bool IsActive);
