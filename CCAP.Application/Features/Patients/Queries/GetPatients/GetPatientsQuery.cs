using MediatR;
using CCAP.Application.Features.Patients.DTOs;

namespace CCAP.Application.Features.Patients.Queries.GetPatients;

public sealed record GetPatientsQuery : IRequest<List<PatientListItemDto>>;
