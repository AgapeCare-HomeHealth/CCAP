using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UpdatePatient;
public sealed record UpdatePatientCommand(Guid PatientId, string MRN, string FirstName, string? MiddleName, string LastName, DateOnly? SocDate, Guid UpdatedByUserId) : IRequest;
