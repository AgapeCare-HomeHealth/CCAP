using CCAP.Application.Features.Dashboard.DTOs;

namespace CCAP.Application.Abstractions.Persistence;

public interface IDashboardRepository
{
    Task<DashboardDto> GetDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken);
}