namespace CCAP.API.Contracts.Admin;

public sealed record CreateServiceTypeRequest(string Code, string Name, string Icon, string CssClass, bool IsActive = true);
public sealed record UpdateServiceTypeRequest(string Code, string Name, string Icon, string CssClass, bool IsActive);
