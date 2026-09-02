using CCAP.Domain.Entities;

namespace CCAP.Application.Features.ReferralDrafts.ReadModels;

public sealed record ReferralDraftListReadModel(
    ReferralDraft Draft,
    string CreatedByName);