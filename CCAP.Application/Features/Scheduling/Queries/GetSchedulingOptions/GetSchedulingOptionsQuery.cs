using CCAP.Application.Features.Admin.LookupOptions;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Queries.GetSchedulingOptions;

public sealed record GetSchedulingOptionsQuery() : IRequest<SchedulingOptionsDto>;

public sealed record SchedulingOptionsDto(
    IReadOnlyList<SchedulingClinicianDto> Clinicians,
    IReadOnlyList<LookupOptionDto> ConfirmationStatuses,
    IReadOnlyList<LookupOptionDto> VisitTypes,
    IReadOnlyList<LookupOptionDto> VisitStatuses);

public sealed record SchedulingClinicianDto(
    Guid UserId,
    string FirstName,
    string LastName)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
