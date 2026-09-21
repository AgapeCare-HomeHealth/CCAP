using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteTask;

public sealed class CompleteTaskCommandValidator : IRequestValidator<CompleteTaskCommand>
{
    public void Validate(CompleteTaskCommand request)
    {
        if (request.TaskId == Guid.Empty) throw new ArgumentException("Task ID is required.");
        if (request.CompletedByUserId == Guid.Empty) throw new ArgumentException("Completed by user ID is required.");
    }
}
