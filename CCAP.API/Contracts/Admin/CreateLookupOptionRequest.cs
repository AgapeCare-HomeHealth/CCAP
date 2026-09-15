namespace CCAP.API.Contracts.Admin;
public sealed record CreateLookupOptionRequest(string LookupType,string Code,string DisplayName,int SortOrder,bool IsActive);
