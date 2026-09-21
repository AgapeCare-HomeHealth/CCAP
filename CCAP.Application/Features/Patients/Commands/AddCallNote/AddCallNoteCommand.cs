using MediatR;

namespace CCAP.Application.Features.Patients.Commands.AddCallNote;

public sealed record AddCallNoteCommand(
    Guid PatientId,
    Guid RecordedByUserId,
    string ContactType,
    string Method,
    string Subject,
    string Notes,
    string? Outcome) : IRequest<Guid>;
