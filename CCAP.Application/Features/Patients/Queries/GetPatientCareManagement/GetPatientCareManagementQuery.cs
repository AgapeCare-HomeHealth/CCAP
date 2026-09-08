using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientCareManagement;

public sealed record GetPatientCareManagementQuery(Guid PatientId)
    : IRequest<PatientCareProfileResponseDto?>;