namespace CCAP.Application.Features.Admin.ServiceTypes;

public sealed record ServiceTypeAdminDto(Guid ServiceTypeId, string Code, string Name, string Icon, string CssClass, bool IsActive);
