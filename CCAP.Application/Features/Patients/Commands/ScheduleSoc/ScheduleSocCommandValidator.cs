using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.ScheduleSoc;

public sealed class ScheduleSocCommandValidator : IRequestValidator<ScheduleSocCommand>
{
    public void Validate(ScheduleSocCommand request)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.");
        if (request.ScheduledByUserId == Guid.Empty)
            throw new ArgumentException("Scheduled by user ID is required.");
        if (request.SocDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("SOC date cannot be in the past.");
    }
}
