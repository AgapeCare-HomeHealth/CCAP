using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class CallNoteRepository : ICallNoteRepository
{
    private readonly AppDbContext _context;
    public CallNoteRepository(AppDbContext context) => _context = context;

    public Task AddAsync(CallNote note, CancellationToken cancellationToken) =>
        _context.CallNotes.AddAsync(note, cancellationToken).AsTask();

    public Task<List<CallNote>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken) =>
        _context.CallNotes
            .Include(x => x.RecordedBy)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CallDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
