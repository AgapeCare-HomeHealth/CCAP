using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Dashboard.DTOs;
using MediatR;

namespace CCAP.Application.Features.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardQueryHandler(
        IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<DashboardDto> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        return _dashboardRepository.GetDashboardAsync(
            request.UserId,
            cancellationToken);
    }
}