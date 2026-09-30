using CCAP.Application.Common.Validation;
namespace CCAP.Application.Features.Patients.Commands.AddServiceOrder;
public sealed class AddServiceOrderCommandValidator : IRequestValidator<AddServiceOrderCommand>
{
 public void Validate(AddServiceOrderCommand r){
  if(r.PatientId==Guid.Empty) throw new ArgumentException("Patient ID is required.");
  if(r.ServiceTypeId==Guid.Empty) throw new ArgumentException("Service type is required.");
  if(r.RecordedByUserId==Guid.Empty) throw new ArgumentException("Recorded by user ID is required.");
  if((r.Frequency?.Length??0)>100) throw new ArgumentException("Frequency cannot exceed 100 characters.");
  if((r.Duration?.Length??0)>100) throw new ArgumentException("Duration cannot exceed 100 characters.");
 }
}
