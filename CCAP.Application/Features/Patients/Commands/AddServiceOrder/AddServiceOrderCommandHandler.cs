using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;
using System.Text.Json;
namespace CCAP.Application.Features.Patients.Commands.AddServiceOrder;
public sealed class AddServiceOrderCommandHandler : IRequestHandler<AddServiceOrderCommand, Guid>
{
 private readonly IPatientRepository _patients; private readonly IServiceTypeRepository _serviceTypes; private readonly IPatientAuditLogRepository _audit; private readonly IUnitOfWork _uow;
 public AddServiceOrderCommandHandler(IPatientRepository patients,IServiceTypeRepository serviceTypes,IPatientAuditLogRepository audit,IUnitOfWork uow){_patients=patients;_serviceTypes=serviceTypes;_audit=audit;_uow=uow;}
 public async Task<Guid> Handle(AddServiceOrderCommand r,CancellationToken ct){
  var patient=await _patients.GetByIdForWorkflowUpdateAsync(r.PatientId,ct)??throw new KeyNotFoundException("Patient not found.");
  var types=await _serviceTypes.GetActiveAsync(ct); var type=types.FirstOrDefault(x=>x.ServiceTypeId==r.ServiceTypeId)??throw new KeyNotFoundException("Service type not found.");
  var order=new PatientServiceOrder(r.PatientId,r.ServiceTypeId,r.Frequency,r.Duration,r.IsPrimaryDiscipline); await _serviceTypes.AddOrderAsync(order,ct);
  patient.Activities.Add(new Activity(patient.PatientId,r.RecordedByUserId,"Order","Service order created",$"{type.Name} order was added."));
  await _audit.AddAsync(new PatientAuditLog(patient.PatientId,r.RecordedByUserId,"PatientServiceOrder",order.PatientServiceOrderId.ToString(),"CREATE",null,JsonSerializer.Serialize(new{order.PatientServiceOrderId,ServiceType=type.Name,r.Frequency,r.Duration,r.IsPrimaryDiscipline,Status=order.Status}),"Service order created."),ct);
  await _uow.SaveChangesAsync(ct); return order.PatientServiceOrderId;
 }
}
