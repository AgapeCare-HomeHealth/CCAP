using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.LookupOptions;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Queries.GetSchedulingOptions;

public sealed class GetSchedulingOptionsQueryHandler
    : IRequestHandler<GetSchedulingOptionsQuery, SchedulingOptionsDto>
{
    private readonly IUserRepository _users;
    private readonly ILookupOptionRepository _lookups;

    public GetSchedulingOptionsQueryHandler(
        IUserRepository users,
        ILookupOptionRepository lookups)
    {
        _users = users;
        _lookups = lookups;
    }

    public async Task<SchedulingOptionsDto> Handle(
        GetSchedulingOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        var clinicians = users
            .Where(x =>
                x.IsActive &&
                x.Role is not null &&
                string.Equals(x.Role.RoleName, "Clinician", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new SchedulingClinicianDto(
                x.UserId,
                x.FirstName,
                x.LastName))
            .ToList();

        var confirmationStatuses = await _lookups.GetAsync(
            "ConfirmationStatus", true, cancellationToken);
        var visitTypes = await _lookups.GetAsync(
            "VisitType", true, cancellationToken);
        var visitStatuses = await _lookups.GetAsync(
            "VisitStatus", true, cancellationToken);

        return new SchedulingOptionsDto(
            clinicians,
            confirmationStatuses.OrderBy(x => x.SortOrder).Select(ToDto).ToList(),
            visitTypes.OrderBy(x => x.SortOrder).Select(ToDto).ToList(),
            visitStatuses.OrderBy(x => x.SortOrder).Select(ToDto).ToList());
    }

    private static LookupOptionDto ToDto(CCAP.Domain.Entities.LookupOption option) =>
        new(
            option.LookupOptionId,
            option.LookupType,
            option.Code,
            option.DisplayName,
            option.SortOrder,
            option.IsActive);
}
