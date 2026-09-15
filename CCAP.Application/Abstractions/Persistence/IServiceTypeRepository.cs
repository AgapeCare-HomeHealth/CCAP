using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IServiceTypeRepository
{
    Task<List<ServiceType>> GetActiveAsync(CancellationToken cancellationToken);
    Task<List<ServiceType>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken);
    Task<bool> HasOrdersAsync(Guid serviceTypeId, CancellationToken cancellationToken);
    Task AddAsync(ServiceType serviceType, CancellationToken cancellationToken);
    void Remove(ServiceType serviceType);
    Task<List<PatientServiceOrder>> GetOrdersByPatientIdAsync(Guid patientId, CancellationToken cancellationToken);
    Task AddOrderAsync(PatientServiceOrder order, CancellationToken cancellationToken);
}
