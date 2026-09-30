using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly AppDbContext _context;
    public ServiceTypeRepository(AppDbContext context) => _context = context;

    public Task<List<ServiceType>> GetActiveAsync(CancellationToken cancellationToken) =>
        _context.ServiceTypes.Where(x => x.IsActive).AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<List<ServiceType>> GetAllAsync(CancellationToken cancellationToken) =>
        _context.ServiceTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<ServiceType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.ServiceTypes.FirstOrDefaultAsync(x => x.ServiceTypeId == id, cancellationToken);

    public Task<bool> ExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken) =>
        _context.ServiceTypes.AnyAsync(x => x.Code == code && (!excludeId.HasValue || x.ServiceTypeId != excludeId.Value), cancellationToken);

    public Task<bool> HasOrdersAsync(Guid serviceTypeId, CancellationToken cancellationToken) =>
        _context.PatientServiceOrders.AnyAsync(x => x.ServiceTypeId == serviceTypeId, cancellationToken);

    public Task AddAsync(ServiceType serviceType, CancellationToken cancellationToken)
    {
        _context.ServiceTypes.Add(serviceType);
        return Task.CompletedTask;
    }

    public void Remove(ServiceType serviceType) => _context.ServiceTypes.Remove(serviceType);

    public Task<List<PatientServiceOrder>> GetOrdersByPatientIdAsync(Guid patientId, CancellationToken cancellationToken) =>
        _context.PatientServiceOrders.Include(x => x.ServiceType).Where(x => x.PatientId == patientId).AsNoTracking().ToListAsync(cancellationToken);

    public Task AddOrderAsync(PatientServiceOrder order, CancellationToken cancellationToken) =>
        _context.PatientServiceOrders.AddAsync(order, cancellationToken).AsTask();
}
