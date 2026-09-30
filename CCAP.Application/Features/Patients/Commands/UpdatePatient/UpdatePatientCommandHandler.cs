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

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { patient.MRN, patient.FirstName, patient.MiddleName, patient.LastName, patient.SocDate });
        patient.UpdateBasicInformation(request.MRN, request.FirstName, request.MiddleName, request.LastName, request.SocDate);
        var newValues = System.Text.Json.JsonSerializer.Serialize(new { patient.MRN, patient.FirstName, patient.MiddleName, patient.LastName, patient.SocDate });
        patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId, request.UpdatedByUserId, "Patient", "Patient information updated", "Patient demographic information was updated."));
        await _auditLogs.AddAsync(new CCAP.Domain.Entities.PatientAuditLog(patient.PatientId, request.UpdatedByUserId, "Patient", patient.PatientId.ToString(), "UPDATE", oldValues, newValues, "Patient information updated."), cancellationToken);
        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
