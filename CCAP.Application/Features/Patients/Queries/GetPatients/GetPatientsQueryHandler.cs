using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Common.Models;
using CCAP.Application.Features.Patients.DTOs;
using CCAP.Domain.Entities;
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
            pageNumber, pageSize, search, status, request.ClinicianId, request.AssignedUserId,
            sortBy, request.SortDescending, cancellationToken);

        var items = patients.Select(x => new PatientListItemDto(
            x.PatientId,
            $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
            x.MRN,
            !string.IsNullOrWhiteSpace(x.FinalStatus) ? x.FinalStatus :
                x.Status == CCAP.Domain.Enums.PatientStatus.Tif ? "TIF" : x.Status.ToString(),
            GetEpisode(x),
            x.PrimaryDiagnosis ?? string.Empty,
            x.Clinician is null ? string.Empty : $"{x.Clinician.FirstName} {x.Clinician.LastName}",
            x.Coordinator is null ? string.Empty : $"{x.Coordinator.FirstName} {x.Coordinator.LastName}",
            null,
            x.ClinicianId,
            request.AssignedUserId.HasValue && x.CoordinatorId == request.AssignedUserId ? "Primary CC" :
                request.AssignedUserId.HasValue && x.SecondaryCoordinatorId == request.AssignedUserId ? "Secondary CC" :
                request.AssignedUserId.HasValue && x.BackupCoordinatorId == request.AssignedUserId ? "Backup CC" : string.Empty,
            x.SocDate,
            x.RocDate,
            GetLatestRecertDate(x),
            x.TifDate)).ToList();

        return new PagedResult<PatientListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static DateOnly? GetLatestRecertDate(Patient patient)
    {
        try
        {
            var recordDates = System.Text.Json.JsonSerializer
                .Deserialize<List<RecertificationRecordResponseDto>>(patient.RecertificationRecordsJson)?
                .Where(record => record.ReceivedDate.HasValue)
                .Select(record => record.ReceivedDate!.Value)
                .ToList() ?? [];

            if (patient.RecertDate.HasValue)
                recordDates.Add(patient.RecertDate.Value);

            return recordDates.Count == 0 ? null : recordDates.Max();
        }
        catch (System.Text.Json.JsonException)
        {
            return patient.RecertDate;
        }
    }

    private static string GetEpisode(Patient patient)
    {
        if (string.Equals(patient.FinalStatus, "Discharged", StringComparison.OrdinalIgnoreCase))
            return "DISCHARGED";

        var latestRecertDate = GetLatestRecertDate(patient);
        DateOnly? latestTifOrRocDate = new DateOnly?[] { patient.TifDate, patient.RocDate }.Max();

        if (latestRecertDate.HasValue &&
            (!latestTifOrRocDate.HasValue || latestRecertDate.Value > latestTifOrRocDate.Value))
            return "RECERT";

        if (patient.TifDate.HasValue && !patient.RocDate.HasValue)
            return "TIF";

        if (patient.RocDate.HasValue)
            return "ROC";

        var socInProgress = patient.Status == CCAP.Domain.Enums.PatientStatus.Active &&
            !PatientWorkflowStageRules.IsGroupComplete(patient, patient.ComplianceRecords, PatientWorkflowStageRules.Soc);

        return socInProgress ? "SOC" : string.Empty;
    }
}
