using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly AppDbContext _context;
    public ServiceTypeRepository(AppDbContext context) => _context = context;

    public Task<List<ServiceType>> GetActiveAsync(CancellationToken cancellationToken) =>
        _context.ServiceTypes
            .Where(x => x.IsActive)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<List<PatientServiceOrder>> GetOrdersByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken) =>
        _context.PatientServiceOrders
            .Include(x => x.ServiceType)
            .Where(x => x.PatientId == patientId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
