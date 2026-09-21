using CCAP.Application.Common.Models;
using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatients;

public sealed record GetPatientsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null,
    Guid? ClinicianId = null,
    string SortBy = "Name",
    bool SortDescending = false) : IRequest<PagedResult<PatientListItemDto>>;
