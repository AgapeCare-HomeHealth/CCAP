using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UpdatePatient;
public sealed record UpdatePatientCommand(
    Guid PatientId, string MRN, string FirstName, string? MiddleName, string LastName, DateOnly? SocDate,
    DateOnly? DateOfBirth, string? Gender, string? PrimaryPhone, string? AlternatePhone,
    string? StreetAddress, string? City, string? State, string? ZipCode,
    string? EmergencyContactName, string? EmergencyContactRelationship, string? EmergencyContactPhone,
    DateTime? ReferralDate, string? ReferralType, string? ReferralSource, string? ReferralChannel,
    string? Priority, string? ReferringPhysician, string? PhysicianPhone, string? PhysicianFax,
    string? PhysicianNpi, string? PcosRegistered, string? PrimaryDiagnosis, string? SecondaryDiagnosis,
    string? ReferralNotes, string? VisitPriority, string? CaseStatus, string? InternalNotes,
    Guid UpdatedByUserId) : IRequest;
