using CCAP.Application.Common.Validation;
namespace CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;
public sealed class UpdateWorkflowDetailsCommandValidator : IRequestValidator<UpdateWorkflowDetailsCommand>{ public void Validate(UpdateWorkflowDetailsCommand r){ if(r.PatientId==Guid.Empty) throw new ArgumentException("Patient ID is required."); if(r.UpdatedByUserId==Guid.Empty) throw new ArgumentException("Updated by user ID is required."); if(r.NumberOfVisits<0) throw new ArgumentException("Number of visits cannot be negative."); }}
