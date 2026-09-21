using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteTask;

public sealed record CompleteTaskCommand(Guid TaskId, Guid CompletedByUserId) : IRequest;
