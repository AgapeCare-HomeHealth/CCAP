using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using CCAP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class PatientTaskRepository
    : IPatientTaskRepository
{
    private readonly AppDbContext _context;

    public PatientTaskRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(PatientTask task, CancellationToken cancellationToken) =>
        _context.PatientTasks.AddAsync(task, cancellationToken).AsTask();

    public async Task<IReadOnlyList<PatientTask>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
        await _context.PatientTasks.Where(x => x.PatientId == patientId).ToListAsync(cancellationToken);

    public Task<PatientTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        _context.PatientTasks.FirstOrDefaultAsync(x => x.TaskId == taskId, cancellationToken);

    public Task<PatientTask?> GetByPatientAndTitleAsync(Guid patientId, string title, CancellationToken cancellationToken) =>
        _context.PatientTasks.FirstOrDefaultAsync(x => x.PatientId == patientId && x.Title == title, cancellationToken);

    public Task<PatientTask?> GetPendingByPatientAndTitleAsync(Guid patientId, string title, CancellationToken cancellationToken) =>
        _context.PatientTasks.FirstOrDefaultAsync(x => x.PatientId == patientId && x.Title == title && x.Status != PatientTaskStatus.Completed && x.Status != PatientTaskStatus.Cancelled, cancellationToken);
}