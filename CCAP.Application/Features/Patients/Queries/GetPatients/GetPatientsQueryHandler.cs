using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;

namespace CCAP.Application.Features.Patients.Queries.GetPatients;

public sealed class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<PatientListItemDto>>
{
    private readonly IPatientRepository _patients;
    public GetPatientsQueryHandler(IPatientRepository patients) => _patients = patients;

    public async Task<List<PatientListItemDto>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var patients = await _patients.GetAllAsync(cancellationToken);

        return patients.Select(x => new PatientListItemDto(
            x.PatientId,
            $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
            x.MRN,
            x.Status.ToString(),
            x.PrimaryDiagnosis ?? string.Empty,
            x.Clinician is null
                ? string.Empty
                : $"{x.Clinician.FirstName} {x.Clinician.LastName}",
            null)).ToList();
    }
}
