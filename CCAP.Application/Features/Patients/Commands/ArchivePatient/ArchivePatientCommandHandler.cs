using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Patients.Commands.ArchivePatient;

public sealed class ArchivePatientCommandHandler : IRequestHandler<ArchivePatientCommand>
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
        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.Archive(request.ArchivedByUserId);

        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
