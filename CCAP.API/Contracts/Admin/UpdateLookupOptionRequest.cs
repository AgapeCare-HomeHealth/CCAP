namespace CCAP.API.Contracts.Admin;
public sealed record UpdateLookupOptionRequest(string Code,string DisplayName,int SortOrder,bool IsActive);
