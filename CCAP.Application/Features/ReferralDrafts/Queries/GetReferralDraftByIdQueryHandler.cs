using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.ReferralDrafts.DTOs;
using MediatR;

namespace CCAP.Application.Features.ReferralDrafts.Queries.GetReferralDraftById;

public sealed class GetReferralDraftByIdQueryHandler
    : IRequestHandler<
        GetReferralDraftByIdQuery,
        ReferralDraftDetailsDto?>
{
    private readonly IReferralDraftRepository _repository;

    public GetReferralDraftByIdQueryHandler(
        IReferralDraftRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReferralDraftDetailsDto?> Handle(
    GetReferralDraftByIdQuery request,
    CancellationToken cancellationToken)
    {
        var draft =
            await _repository.GetByIdAsync(
                request.ReferralDraftId,
                cancellationToken);

        if (draft is null)
        {
            return null;
        }

        return new ReferralDraftDetailsDto
        {
            ReferralDraftId =
                draft.ReferralDraftId,

            Data =
                draft.Data
        };
    }
}