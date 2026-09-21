using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Enums;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteTask;

public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand>
{
    private readonly IPatientTaskRepository _tasks;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTaskCommandHandler(IPatientTaskRepository tasks, IUnitOfWork unitOfWork)
    {
        _tasks = tasks;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (request.TaskId == Guid.Empty)
            throw new ArgumentException("Task ID is required.");
        if (request.CompletedByUserId == Guid.Empty)
            throw new ArgumentException("Completed by user ID is required.");

        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient task was not found.");

        if (task.Status == PatientTaskStatus.Completed) return;
        if (task.AssignedUserId.HasValue && task.AssignedUserId.Value != request.CompletedByUserId)
            throw new UnauthorizedAccessException("You are not assigned to this task.");
        if (task.Status == PatientTaskStatus.Cancelled)
            throw new InvalidOperationException("Cancelled tasks cannot be completed.");

        if (task.Title.Contains("Verify Insurance", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Complete insurance verification before completing this task.");

        if (task.Title.Contains("Schedule SOC", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Schedule the SOC visit before completing this task.");

        if (task.Title.Contains("Complete SOC", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Complete the SOC visit before completing this task.");

        task.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
