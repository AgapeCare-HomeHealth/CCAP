using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(
        AppDbContext context)
    {
        _context = context;
    }


    public Task<Patient?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _context.Patients

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

            .Include(x => x.ComplianceRecords)

            .Include(x => x.Visits)
                .ThenInclude(x => x.Clinician)

            .Include(x => x.Activities)
                .ThenInclude(x => x.PerformedBy)

            .AsNoTracking()

            .FirstOrDefaultAsync(
                x => x.PatientId == id,
                cancellationToken);
    }


    public async Task<Patient?> GetByIdForUpdateAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(
                x => x.PatientId == patientId,
                cancellationToken);
    }


    public async Task<Patient?> GetByIdForWorkflowUpdateAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await _context.Patients

            .Include(x => x.Tasks)

            .Include(x => x.ComplianceRecords)

            .Include(x => x.Visits)

            .Include(x => x.Activities)

            .FirstOrDefaultAsync(
                x => x.PatientId == patientId,
                cancellationToken);
    }


    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? status,
        Guid? clinicianId,
        string sortBy,
        bool sortDescending,
        CancellationToken cancellationToken)
    {
        var query = _context.Patients
            .Include(x => x.Clinician)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.FirstName.Contains(term) ||
                (x.MiddleName != null && x.MiddleName.Contains(term)) ||
                x.LastName.Contains(term) ||
                x.MRN.Contains(term) ||
                (x.PrimaryDiagnosis != null && x.PrimaryDiagnosis.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CCAP.Domain.Enums.PatientStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);

        if (clinicianId.HasValue && clinicianId.Value != Guid.Empty)
            query = query.Where(x => x.ClinicianId == clinicianId.Value);

        query = sortBy.ToLowerInvariant() switch
        {
            "mrn" => sortDescending ? query.OrderByDescending(x => x.MRN) : query.OrderBy(x => x.MRN),
            "status" => sortDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "diagnosis" => sortDescending ? query.OrderByDescending(x => x.PrimaryDiagnosis) : query.OrderBy(x => x.PrimaryDiagnosis),
            "clinician" => sortDescending ? query.OrderByDescending(x => x.Clinician!.LastName).ThenByDescending(x => x.Clinician!.FirstName) : query.OrderBy(x => x.Clinician!.LastName).ThenBy(x => x.Clinician!.FirstName),
            _ => sortDescending ? query.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName) : query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<List<Patient>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return _context.Patients

            .Include(x => x.Clinician)

            .AsNoTracking()

            .OrderBy(x => x.LastName)

            .ThenBy(x => x.FirstName)

            .ToListAsync(cancellationToken);
    }


    public Task<Patient?> GetByMrnAsync(
        string mrn,
        CancellationToken cancellationToken)
    {
        return _context.Patients

            .FirstOrDefaultAsync(
                x => x.MRN == mrn.Trim(),
                cancellationToken);
    }


    public Task AddAsync(
        Patient patient,
        CancellationToken cancellationToken)
    {
        return _context.Patients
            .AddAsync(
                patient,
                cancellationToken)
            .AsTask();
    }


    public void Update(
        Patient patient)
    {
        _context.Patients.Update(patient);
    }
}