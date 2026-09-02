using System.Text.Json;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;

public sealed class SaveReferralDraftCommandHandler
    : IRequestHandler<
        SaveReferralDraftCommand,
        SaveReferralDraftResult>
{
    private readonly IReferralDraftRepository _drafts;
    private readonly IUnitOfWork _unitOfWork;

    public SaveReferralDraftCommandHandler(
        IReferralDraftRepository drafts,
        IUnitOfWork unitOfWork)
    {
        _drafts = drafts;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaveReferralDraftResult> Handle(
        SaveReferralDraftCommand request,
        CancellationToken cancellationToken)
    {
        SaveReferralDraftValidator.Validate(
            request);

        // =====================================================
        // UPDATE EXISTING DRAFT
        // =====================================================

        if (request.ReferralDraftId.HasValue)
        {
            var existing =
                await _drafts.GetByIdAsync(
                    request.ReferralDraftId.Value,
                    cancellationToken);

            if (existing is null)
            {
                throw new KeyNotFoundException(
                    "Referral draft was not found.");
            }

            existing.Update(
                request.Data);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new SaveReferralDraftResult(
                existing.ReferralDraftId,
                existing.Status.ToString());
        }


        // =====================================================
        // CREATE NEW DRAFT
        // =====================================================

        var draft =
            new ReferralDraft(
                request.CreatedByUserId,
                request.Data);

        await _drafts.AddAsync(
            draft,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new SaveReferralDraftResult(
            draft.ReferralDraftId,
            draft.Status.ToString());
    }
}