using System.Text.Json;
using CCAP.Application.Features.Patients;
using CCAP.API.Controllers;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using CCAP.Domain.Entities;
using CCAP.Domain.Enums;
using CCAP.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCAP.API.Services;

public sealed class PatientImportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PatientImportBackgroundService> _logger;
    public PatientImportBackgroundService(IServiceScopeFactory scopeFactory, ILogger<PatientImportBackgroundService> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessNextBatch(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Patient import worker iteration failed."); }
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task ProcessNextBatch(CancellationToken ct)
    {
        Guid batchId;
        List<Guid> rowIds;
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var batch = await db.PatientImportBatches
                .Where(x => x.Status == "Queued" || x.Status == "Processing")
                .OrderBy(x => x.UploadedAt).FirstOrDefaultAsync(ct);
            if (batch is null) return;
            batch.Start();
            batchId = batch.PatientImportBatchId;
            await db.SaveChangesAsync(ct);
            rowIds = await db.PatientImportRows.Where(x => x.PatientImportBatchId == batchId &&
                    (x.Status == "Ready" || x.Status == "Processing"))
                .OrderBy(x => x.RowNumber).Select(x => x.PatientImportRowId).ToListAsync(ct);
        }

        foreach (var rowId in rowIds)
        {
            if (ct.IsCancellationRequested) return;
            await ProcessOneRow(batchId, rowId, ct);
        }

        // Reconcile counts even if the process restarted after the last row commit.
        using var finalScope = _scopeFactory.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var finalBatch = await finalDb.PatientImportBatches.FirstOrDefaultAsync(x => x.PatientImportBatchId == batchId, ct);
        if (finalBatch is null) return;
        var finalCounts = await finalDb.PatientImportRows.Where(x => x.PatientImportBatchId == batchId)
            .GroupBy(_ => 1).Select(g => new
            {
                Inserted = g.Count(x => x.Status == "Inserted"),
                Duplicates = g.Count(x => x.Status == "Duplicate"),
                Failed = g.Count(x => x.Status == "Failed"),
                Total = g.Count()
            }).FirstOrDefaultAsync(ct);
        if (finalCounts is not null) finalBatch.RefreshCounts(finalCounts.Inserted, finalCounts.Duplicates, finalCounts.Failed, finalCounts.Total);
        await finalDb.SaveChangesAsync(ct);
    }

    private async Task ProcessOneRow(Guid batchId, Guid rowId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var batch = await db.PatientImportBatches.FirstAsync(x => x.PatientImportBatchId == batchId, ct);
        var row = await db.PatientImportRows.FirstAsync(x => x.PatientImportRowId == rowId, ct);
        if (row.Status is not ("Ready" or "Processing")) return;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            row.MarkProcessing();
            await db.SaveChangesAsync(ct);
            await ProcessRow(db, sender, row, batch.UploadedByUserId, ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            return;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogWarning(ex, "Patient import row {RowNumber} in batch {BatchId} failed.", row.RowNumber, batchId);
            db.ChangeTracker.Clear();
            var failedRow = await db.PatientImportRows.FirstAsync(x => x.PatientImportRowId == rowId, CancellationToken.None);
            failedRow.MarkFailed(TrimError(ex.Message));
            await db.SaveChangesAsync(CancellationToken.None);
            batch = await db.PatientImportBatches.FirstAsync(x => x.PatientImportBatchId == batchId, CancellationToken.None);
        }

        var counts = await db.PatientImportRows.Where(x => x.PatientImportBatchId == batchId)
            .GroupBy(_ => 1).Select(g => new
            {
                Inserted = g.Count(x => x.Status == "Inserted"),
                Duplicates = g.Count(x => x.Status == "Duplicate"),
                Failed = g.Count(x => x.Status == "Failed"),
                Total = g.Count()
            }).FirstOrDefaultAsync(ct);
        if (counts is not null) batch.RefreshCounts(counts.Inserted, counts.Duplicates, counts.Failed, counts.Total);
        await db.SaveChangesAsync(ct);
    }

    private static async Task ProcessRow(AppDbContext db, ISender sender, PatientImportRow row, Guid uploadedBy, CancellationToken ct)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(row.RawDataJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        string V(string key) => values.GetValueOrDefault(key, "").Trim();
        var alreadyImported = await db.PatientImportRows.AnyAsync(x => x.PatientImportRowId != row.PatientImportRowId &&
            x.RecordFingerprint == row.RecordFingerprint && x.Status == "Inserted", ct);
        if (alreadyImported)
        {
            row.MarkDuplicate("This exact patient data was successfully imported in another batch.");
            return;
        }
        var coordinatorEmail = V("CoordinatorEmail");
        var coordinator = await db.ApplicationUsers.Include(x => x.Role).FirstOrDefaultAsync(x => x.IsActive && x.Email == coordinatorEmail && x.Role.RoleName == "Care Coordinator", ct)
            ?? throw new InvalidOperationException($"No active Care Coordinator was found for '{coordinatorEmail}'.");
        var clinicianName = V("AssignedClinician");
        ApplicationUser? clinician = null;
        if (!string.IsNullOrWhiteSpace(clinicianName))
        {
            var activeUsers = await db.ApplicationUsers.Include(x => x.Role).Where(x => x.IsActive).ToListAsync(ct);
            var clinicianMatches = PatientImportController.ResolveClinician(activeUsers, clinicianName).ToList();
            if (clinicianMatches.Count == 0) throw new InvalidOperationException($"No active clinician matched '{clinicianName}'.");
            if (clinicianMatches.Count > 1) throw new InvalidOperationException($"Clinician '{clinicianName}' matches more than one active user. Use a unique clinician name.");
            clinician = clinicianMatches[0];
        }

        var patient = row.PatientId.HasValue
            ? await db.Patients.FirstOrDefaultAsync(x => x.PatientId == row.PatientId.Value, ct)
            : await db.Patients.FirstOrDefaultAsync(x => x.MRN == row.GeneratedMrn, ct);
        var referral = row.ReferralId.HasValue
            ? await db.Referrals.FirstOrDefaultAsync(x => x.ReferralId == row.ReferralId.Value, ct)
            : await db.Referrals.FirstOrDefaultAsync(x => x.ReferralNumber == row.GeneratedReferralNumber, ct);
        if (patient is null && referral is not null) throw new InvalidOperationException("Import recovery found a referral without its patient. Manual review is required.");
        if (patient is null)
        {
            var command = new CreateReferralIntakeCommand(
                ReferralDraftId: null,
                MRN: row.GeneratedMrn!,
                FirstName: V("FirstName"),
                MiddleName: "",
                LastName: V("LastName"),
                DateOfBirth: ParseDate(V("DateOfBirth")) ?? DateOnly.FromDateTime(DateTime.Today),
                Gender: null,
                PrimaryPhone: null,
                AlternatePhone: null,
                StreetAddress: null,
                City: null,
                State: null,
                ZipCode: null,
                EmergencyContactName: null,
                EmergencyContactRelationship: null,
                EmergencyContactPhone: null,
                EmergencyContactsJson: null,
                ReferralNumber: row.GeneratedReferralNumber!,
                ReferralDate: DateTime.Today,
                ReferralSource: "Bulk Patient Import",
                ReferralType: "Home Health",
                ReferralChannel: "CSV Import",
                Priority: "Normal",
                PrimaryInsurance: NullIfEmpty(V("PayorInsurance")),
                InsuranceMemberId: null,
                AuthorizationDate: null,
                ApprovedVisits: null,
                AuthorizationRequired: false,
                ReferringPhysician: null,
                PhysicianPhone: null,
                PhysicianFax: null,
                PhysicianNpi: null,
                PcosRegistered: null,
                PrimaryDiagnosis: null,
                SecondaryDiagnosis: null,
                OrderedServices: Array.Empty<string>(),
                ReferralNotes: NullIfEmpty(V("NotesRemarks")),
                CoordinatorId: coordinator.UserId,
                SecondaryCoordinatorId: null,
                ClinicianId: clinician?.UserId,
                DisciplineId: clinician?.DisciplineId,
                SocDate: null,
                VisitPriority: null,
                CaseStatus: NullIfEmpty(V("PatientStatus")),
                InternalNotes: NullIfEmpty(V("NotesRemarks")),
                PdfStream: null,
                PdfFileName: null,
                PdfContentType: null,
                PdfSize: null);
            var result = await sender.Send(command, ct);
            patient = await db.Patients.FirstAsync(x => x.PatientId == result.PatientId, ct);
            referral = await db.Referrals.FirstAsync(x => x.ReferralId == result.ReferralId, ct);
        }
        if (patient is null || referral is null) throw new InvalidOperationException("The imported patient or referral could not be recovered.");
        row.AttachCreatedRecords(patient.PatientId, referral.ReferralId);
        await db.SaveChangesAsync(ct);

        // Re-apply only fields supplied by this import row. Existing values not
        // represented by the CSV are preserved when a failed row is retried.
        patient.SetCoordinatorAssignments(coordinator.UserId,
            patient.SecondaryCoordinatorId == coordinator.UserId ? null : patient.SecondaryCoordinatorId);
        if (!string.IsNullOrWhiteSpace(clinicianName)) patient.SetClinician(clinician?.UserId);
        if (string.Equals(V("PatientStatus"), "Active", StringComparison.OrdinalIgnoreCase)) patient.ActivateAfterAdmission();
        patient.UpdateInsurance(NullIfEmpty(V("PayorInsurance")) ?? patient.PrimaryInsurance, patient.InsuranceMemberId,
            patient.AuthorizationDate, patient.ApprovedVisits, patient.AuthorizationRequired);
        patient.UpdateWorkflowDetails(
            patient.PreAuthDueDate, patient.NumberOfVisits, NullIfEmpty(V("CaseMixType")) ?? patient.CaseMixType,
            patient.HospitalDcSummaryNotes, NullIfEmpty(V("DMEMedSupply")) ?? patient.DmeMedSupplyNotes,
            NullIfEmpty(V("SOCFeedbackFromPatient")) ?? patient.SocFeedbackFromPatient,
            ParseDate(V("TIF")) ?? patient.TifDate, ParseDate(V("ROC")) ?? patient.RocDate,
            ParseDate(V("Recert")) ?? patient.RecertDate, ParseYesNo(V("PCPPTNotified")) ?? patient.PcpPtNotified,
            ParseDate(V("DischargeDate")) ?? patient.DischargeDate,
            NullIfEmpty(V("DCFeedbackFromPatient")) ?? patient.DischargeFeedback,
            patient.TransferDestination, patient.TransferDate, patient.TransferReason, patient.HighLevelOfCareSpecialNeeds);

        if (string.Equals(V("PatientStatus"), "TIF", StringComparison.OrdinalIgnoreCase))
            patient.MarkAsTif();

        if (string.Equals(V("PatientStatus"), "Discharged", StringComparison.OrdinalIgnoreCase))
            patient.MarkImportedAsDischarged(uploadedBy);

        var complianceMap = new (string Csv, string Code)[]
        {
            ("PreAuthReceived", "PRE_AUTH_RECEIVED"), ("PROReceived", "PRO_RECEIVED"),
            ("HPReceived", "H_AND_P_RECEIVED"),
            ("F2FReceived", "F2F_RECEIVED"), ("PCPConfirmed", "PCP_CONFIRMED"),
            ("DMEMedSupply", "DME_MED_SUPPLY"), ("SignedConsents", "SIGNED_CONSENTS"),
            ("NOA", "NOA_FILED"), ("OASISSOCCompliant", "OASIS_SOC_COMPLETE"),
            ("AVSReceived", "AVS_RECEIVED"),
            ("CaseMixIdentified", "CASE_MIX_IDENTIFIED"), ("CaseMixPlotted", "CASE_MIX_PLOTTED"),
            ("QAApprovalPOPOCReadyForFaxing", "QA_APPROVAL"), ("POCCreated", "POC_CREATED"),
            ("POCSigned", "POC_SIGNED"),
            ("OrdersSignedByPhysician", "ORDERS_SIGNED"), ("DocsUploaded", "DOCS_UPLOADED"),
            ("DCSummarySigned", "DISCHARGE_SUMMARY_SIGNED"),
            ("NOMNCSigned", "NOMNC_SIGNED")
        };
        var records = await db.ComplianceRecords.Where(x => x.PatientId == patient.PatientId).ToListAsync(ct);
        foreach (var (code, description) in new[]
        {
            ("POC_CREATED", "Confirm the plan of care has been created."),
            ("POC_SIGNED", "Confirm the plan of care has been signed.")
        })
        {
            if (records.Any(x => x.RequirementCode.Equals(code, StringComparison.OrdinalIgnoreCase)))
                continue;

            var record = new ComplianceRecord(patient.PatientId, code, description);
            db.ComplianceRecords.Add(record);
            records.Add(record);
        }

        var statuses = complianceMap.ToDictionary(x => x.Code, x => NormalizeStatus(V(x.Csv)), StringComparer.OrdinalIgnoreCase);
        var ordered = complianceMap.Select(x => x.Code).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            if (statuses[ordered[i]] is null && ordered.Skip(i + 1).Any(code => statuses[code] is "YES" or "NO")) statuses[ordered[i]] = "N/A";
        }
        foreach (var mapping in complianceMap)
        {
            var status = statuses[mapping.Code];
            if (status is null) continue;
            var record = records.FirstOrDefault(x => x.RequirementCode.Equals(mapping.Code, StringComparison.OrdinalIgnoreCase));
            if (record is null) continue;
            record.SetStatus(status, uploadedBy);
            var title = PatientComplianceTaskCatalog.TaskTitle(mapping.Code);
            var task = await db.PatientTasks.FirstOrDefaultAsync(x => x.PatientId == patient.PatientId && x.Title == title, ct);
            if (task is not null && task.Status != PatientTaskStatus.Cancelled)
            {
                if (status is "YES" or "N/A") task.Complete();
                else if (task.Status == PatientTaskStatus.Completed) task.Reopen();
            }
        }

        if (!string.IsNullOrWhiteSpace(V("PayorInsurance")))
        {
            patient.VerifyInsurance(uploadedBy);
            var insurance = records.FirstOrDefault(x => x.RequirementCode == "INSURANCE_VERIFICATION");
            insurance?.SetStatus("YES", uploadedBy);
            var task = await db.PatientTasks.FirstOrDefaultAsync(x => x.PatientId == patient.PatientId && x.Title == "Verify Insurance", ct);
            if (task is not null && task.Status != PatientTaskStatus.Cancelled) task.Complete();
            db.PatientAuditLogs.Add(new PatientAuditLog(patient.PatientId, uploadedBy, "InsuranceVerification", patient.PatientId.ToString(), "UPDATE", null,
                JsonSerializer.Serialize(new { patient.PrimaryInsurance, VerifiedAt = DateTime.UtcNow, VerifiedByUserId = uploadedBy, Source = "Patient CSV Import" }),
                "Insurance verification automatically completed because a payor value was supplied during patient import."));
        }

        await db.SaveChangesAsync(ct);
        row.MarkSucceeded(patient.PatientId, referral.ReferralId);
    }

    private static string? NormalizeStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim().ToUpperInvariant() switch { "YES" or "Y" or "TRUE" or "1" or "COMPLETE" or "COMPLETED" => "YES", "NO" or "N" or "FALSE" or "0" => "NO", "N/A" or "NA" => "N/A", _ => null };
    }
    private static DateOnly? ParseDate(string value) => DateOnly.TryParse(value, out var date) ? date : DateTime.TryParse(value, out var dateTime) ? DateOnly.FromDateTime(dateTime) : null;
    private static bool? ParseYesNo(string value) => NormalizeStatus(value) switch { "YES" => true, "NO" => false, _ => null };
    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
    private static string TrimError(string value) => value.Length > 1900 ? value[..1900] : value;
}
