using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed class CompleteCareCommandHandler : IRequestHandler<CompleteCareCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteCareCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteCareCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.CompleteCare(
            request.FinalStatus,
            request.FinalizedByUserId);

        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
