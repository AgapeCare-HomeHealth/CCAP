using MediatR;

namespace CCAP.Application.Features.Patients.Commands.ArchivePatient;

public sealed record ArchivePatientCommand(
    Guid PatientId,
    Guid ArchivedByUserId) : IRequest;
