using CCAP.Application.Features.Dashboard.DTOs;
using MediatR;

namespace CCAP.Application.Features.Dashboard.Queries.GetDashboard;

public sealed record GetDashboardQuery(
    Guid UserId
) : IRequest<DashboardDto>;