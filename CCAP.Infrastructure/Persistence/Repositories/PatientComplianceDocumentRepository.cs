using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class PatientComplianceDocumentRepository : IPatientComplianceDocumentRepository
{
    private readonly AppDbContext _context;

    public PatientComplianceDocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(
        PatientComplianceDocument document,
        CancellationToken cancellationToken) =>
        _context.PatientComplianceDocuments
            .AddAsync(document, cancellationToken)
            .AsTask();

    public Task<PatientComplianceDocument?> GetLatestAsync(
        Guid patientId,
        string requirementCode,
        CancellationToken cancellationToken) =>
        _context.PatientComplianceDocuments
            .AsNoTracking()
            .Where(x =>
                x.PatientId == patientId &&
                x.RequirementCode == requirementCode)
            .OrderByDescending(x => x.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
