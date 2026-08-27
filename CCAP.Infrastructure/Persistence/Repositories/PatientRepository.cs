using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;
    public PatientRepository(AppDbContext context) => _context = context;

    public Task<Patient?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken) =>
    _context.Patients
        .Include(x => x.Coordinator)
        .Include(x => x.Clinician)

        .Include(x => x.Referrals)
            .ThenInclude(x => x.Location)

        .Include(x => x.Referrals)
            .ThenInclude(x => x.Discipline)

        .Include(x => x.Referrals)
            .ThenInclude(x => x.AssignedUser)

        .Include(x => x.Tasks)
            .ThenInclude(x => x.AssignedUser)

        .Include(x => x.Activities)
            .ThenInclude(x => x.PerformedBy)

        .AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken);

    public Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken) =>
        _context.Patients
            .Include(x => x.Clinician)
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Patient?> GetByMrnAsync(
    string mrn,
    CancellationToken cancellationToken) =>
    _context.Patients
        .FirstOrDefaultAsync(
            x => x.MRN == mrn.Trim(),
            cancellationToken);

    public Task AddAsync(Patient patient, CancellationToken cancellationToken) =>
        _context.Patients.AddAsync(patient, cancellationToken).AsTask();

    public void Update(Patient patient) => _context.Patients.Update(patient);
}
