using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;

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

    public Task AddAsync(
        PatientTask task,
        CancellationToken cancellationToken) =>
        _context.PatientTasks
            .AddAsync(
                task,
                cancellationToken)
            .AsTask();
}