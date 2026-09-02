using System.Text.Json;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Common.Models;
using CCAP.Application.Features.ReferralDrafts.DTOs;
using MediatR;

namespace CCAP.Application.Features.ReferralDrafts.Queries;

public sealed class GetReferralDraftsQueryHandler
    : IRequestHandler<
        GetReferralDraftsQuery,
        PagedResult<ReferralDraftListDto>>
{
    private readonly IReferralDraftRepository _repository;

    public GetReferralDraftsQueryHandler(
        IReferralDraftRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ReferralDraftListDto>> Handle(
        GetReferralDraftsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        var pageSize =
            request.PageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => request.PageSize
            };

        var search =
            string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();

        var (drafts, totalCount) =
            await _repository.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                cancellationToken);

        //var items =
        //    drafts
        //        .Select(MapToListDto)
        //        .ToList();

        var items =
            drafts
                .Select(x =>
                    MapToListDto(
                        x.Draft,
                        x.CreatedByName))
                .ToList();

        return new PagedResult<ReferralDraftListDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static ReferralDraftListDto MapToListDto(
    Domain.Entities.ReferralDraft draft,
    string createdByName)
    {
        ReferralDraftListDataDto? data = null;

        try
        {
            data =
                JsonSerializer.Deserialize<ReferralDraftListDataDto>(
                    draft.Data,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (JsonException)
        {
        }

        var patientName =
            string.Join(
                " ",
                new[]
                {
                data?.FirstName,
                data?.MiddleName,
                data?.LastName
                }
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            .Trim();

        return new ReferralDraftListDto
        {
            ReferralDraftId = draft.ReferralDraftId,
            CreatedByUserId = draft.CreatedByUserId,

            // GUID → actual user's name
            SavedByName = createdByName,

            PatientName = patientName,

            ReferralNumber =
                data?.ReferralNumber
                ?? string.Empty,

            Status =
                draft.Status.ToString(),

            CreatedAt =
                draft.CreatedAt,

            UpdatedAt =
                draft.UpdatedAt
        };
    }
}