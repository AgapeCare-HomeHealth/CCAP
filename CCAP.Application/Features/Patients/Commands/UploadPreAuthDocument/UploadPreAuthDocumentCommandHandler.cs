using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Storage;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UploadPreAuthDocument;

public sealed class UploadPreAuthDocumentCommandHandler
    : IRequestHandler<UploadPreAuthDocumentCommand, UploadPreAuthDocumentResult>
{
    private const string RequirementCode = "PRE_AUTH_RECEIVED";
    private readonly IPatientRepository _patients;
    private readonly IPatientComplianceDocumentRepository _documents;
    private readonly IPatientAuditLogRepository _auditLogs;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UploadPreAuthDocumentCommandHandler(
        IPatientRepository patients,
        IPatientComplianceDocumentRepository documents,
        IPatientAuditLogRepository auditLogs,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _documents = documents;
        _auditLogs = auditLogs;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadPreAuthDocumentResult> Handle(
        UploadPreAuthDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (!_fileStorage.CanStore)
            throw new InvalidOperationException(
                "File storage is not enabled. Configure a file storage provider before uploading documents.");

        var safeFileName = Path.GetFileName(request.FileName);
        var now = DateTime.UtcNow;
        var folder = $"Patients/{request.PatientId}/Compliance/{RequirementCode}/{now:yyyy}/{now:MM}";

        var storedFile = await _fileStorage.SaveAsync(
            request.Content,
            safeFileName,
            request.ContentType,
            folder,
            cancellationToken);

        var document = new PatientComplianceDocument(
            request.PatientId,
            RequirementCode,
            storedFile.StorageKey,
            safeFileName,
            request.ContentType,
            request.FileSize,
            request.UploadedByUserId);

        try
        {
            await _documents.AddAsync(document, cancellationToken);

            await _auditLogs.AddAsync(
                new PatientAuditLog(
                    request.PatientId,
                    request.UploadedByUserId,
                    "Compliance Document",
                    document.PatientComplianceDocumentId.ToString(),
                    "CREATE",
                    null,
                    document.OriginalFileName,
                    "Pre-authorization insurance document uploaded."),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(storedFile.StorageKey, cancellationToken);
            throw;
        }

        return new UploadPreAuthDocumentResult(
            document.PatientComplianceDocumentId,
            document.OriginalFileName,
            document.ContentType,
            document.FileSize,
            document.UploadedAt);
    }
}
