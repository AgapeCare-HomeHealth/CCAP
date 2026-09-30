using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface ICallNoteRepository
{
    Task AddAsync(CallNote note, CancellationToken cancellationToken);
    Task<List<CallNote>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken);
}
