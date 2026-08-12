namespace CCAP.Application.Features.Patients.DTOs;

public sealed record ServiceTypeDto(
    Guid ServiceTypeId,
    string Code,
    string Name,
    string Icon,
    string CssClass);
