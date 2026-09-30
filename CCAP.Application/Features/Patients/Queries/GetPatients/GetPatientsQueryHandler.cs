using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Common.Models;
using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatients;

public sealed class GetPatientsQueryHandler
    : IRequestHandler<GetPatientsQuery, PagedResult<PatientListItemDto>>
{
    private readonly IPatientRepository _patients;

    public GetPatientsQueryHandler(IPatientRepository patients) => _patients = patients;

    public async Task<PagedResult<PatientListItemDto>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "Name" : request.SortBy.Trim();

        var (patients, totalCount) = await _patients.GetPagedAsync(
            pageNumber, pageSize, search, status, request.ClinicianId, sortBy, request.SortDescending, cancellationToken);

        var items = patients.Select(x => new PatientListItemDto(
            x.PatientId,
            $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
            x.MRN,
            x.Status.ToString(),
            x.PrimaryDiagnosis ?? string.Empty,
            x.Clinician is null ? string.Empty : $"{x.Clinician.FirstName} {x.Clinician.LastName}",
            null,
            x.ClinicianId)).ToList();

        return new PagedResult<PatientListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
