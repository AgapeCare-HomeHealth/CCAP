using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralListForExport;

public sealed record GetReferralListForExportQuery(
    string? Search = null,
    string? Status = null,
    string? Source = null,
    string? Insurance = null,
    Guid? AssignedCoordinatorId = null) : IRequest<IReadOnlyList<ReferralListItemDto>>;
