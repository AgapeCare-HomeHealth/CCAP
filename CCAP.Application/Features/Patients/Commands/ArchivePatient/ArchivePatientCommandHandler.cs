using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.ArchivePatient;

public sealed class ArchivePatientCommandHandler
    : IRequestHandler<ArchivePatientCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;

    public ArchivePatientCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ArchivePatientCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForUpdateAsync(
            request.PatientId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.Archive(request.ArchivedByUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}