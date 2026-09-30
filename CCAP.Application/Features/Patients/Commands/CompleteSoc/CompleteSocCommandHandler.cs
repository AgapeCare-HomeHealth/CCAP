using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteSoc;

public sealed class CompleteSocCommandHandler
    : IRequestHandler<CompleteSocCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSocCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteSocCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _patients.GetByIdForWorkflowUpdateAsync(
                request.PatientId,
                cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        if (!patient.SocDate.HasValue)
        {
            throw new InvalidOperationException(
                "SOC has not been scheduled.");
        }

        var socDate =
            patient.SocDate.Value
                .ToDateTime(TimeOnly.MinValue)
                .Date;

        var socVisit =
            patient.Visits
                .Where(x =>
                    x.ScheduledDate.Date == socDate)
                .OrderByDescending(x => x.ScheduledDate)
                .FirstOrDefault();

        if (socVisit is null)
        {
            throw new InvalidOperationException(
                "The SOC visit has not been scheduled.");
        }

        if (string.Equals(
                socVisit.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        socVisit.Complete(request.Notes);

        foreach (var task in patient.Tasks.Where(x =>
            x.Status != CCAP.Domain.Enums.PatientTaskStatus.Completed &&
            x.Status != CCAP.Domain.Enums.PatientTaskStatus.Cancelled &&
            x.Title.Contains("Complete SOC", StringComparison.OrdinalIgnoreCase)))
        {
            task.Complete();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}