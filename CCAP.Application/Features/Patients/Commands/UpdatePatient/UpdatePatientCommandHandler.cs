using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UpdatePatient;

public sealed class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientAuditLogRepository _auditLogs;

    public UpdatePatientCommandHandler(IPatientRepository patients, IUnitOfWork unitOfWork, IPatientAuditLogRepository auditLogs)
    { _patients = patients; _unitOfWork = unitOfWork; _auditLogs = auditLogs; }

    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForUpdateAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var duplicate = await _patients.GetByMrnAsync(request.MRN, cancellationToken);
        if (duplicate is not null && duplicate.PatientId != request.PatientId)
            throw new InvalidOperationException($"A patient with MRN '{request.MRN.Trim()}' already exists.");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new {
            patient.MRN, patient.FirstName, patient.MiddleName, patient.LastName, patient.SocDate,
            patient.DateOfBirth, patient.Gender, patient.PhoneNumber, patient.AlternatePhone,
            patient.Address, patient.City, patient.State, patient.ZipCode, patient.PrimaryDiagnosis,
            patient.SecondaryDiagnosis
        });

        patient.UpdateEditableProfile(request.MRN, request.FirstName, request.MiddleName, request.LastName,
            request.SocDate, request.DateOfBirth, request.Gender, request.PrimaryDiagnosis, request.SecondaryDiagnosis,
            request.StreetAddress, request.City, request.State, request.ZipCode, request.PrimaryPhone,
            request.AlternatePhone, request.EmergencyContactName, request.EmergencyContactRelationship,
            request.EmergencyContactPhone, request.ReferringPhysician, request.PhysicianPhone, request.ReferralNotes);

        var referral = patient.Referrals.OrderByDescending(x => x.ReferralDate).FirstOrDefault();
        referral?.UpdateEditableDetails(request.ReferralDate, request.ReferralSource, request.ReferralType,
            request.ReferralChannel, request.Priority, request.VisitPriority, request.CaseStatus,
            request.ReferringPhysician, request.PhysicianPhone, request.PhysicianFax, request.PhysicianNpi,
            request.PcosRegistered, request.SecondaryDiagnosis, request.ReferralNotes, request.InternalNotes);

        var newValues = System.Text.Json.JsonSerializer.Serialize(new {
            patient.MRN, patient.FirstName, patient.MiddleName, patient.LastName, patient.SocDate,
            patient.DateOfBirth, patient.Gender, patient.PhoneNumber, patient.AlternatePhone,
            patient.Address, patient.City, patient.State, patient.ZipCode, patient.PrimaryDiagnosis,
            patient.SecondaryDiagnosis
        });
        patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId, request.UpdatedByUserId,
            "Patient", "Patient information updated", "Patient demographic and referral information was updated."));
        await _auditLogs.AddAsync(new CCAP.Domain.Entities.PatientAuditLog(patient.PatientId, request.UpdatedByUserId,
            "Patient", patient.PatientId.ToString(), "UPDATE", oldValues, newValues, "Patient information updated."), cancellationToken);
        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
