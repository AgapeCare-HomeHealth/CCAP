using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientWorkflow;

public sealed record GetPatientWorkflowQuery(Guid PatientId)
    : IRequest<PatientWorkflowResponseDto?>;