using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IServiceTypeRepository
{
    Task<List<ServiceType>> GetActiveAsync(CancellationToken cancellationToken);
    Task<List<PatientServiceOrder>> GetOrdersByPatientIdAsync(Guid patientId, CancellationToken cancellationToken);
}
