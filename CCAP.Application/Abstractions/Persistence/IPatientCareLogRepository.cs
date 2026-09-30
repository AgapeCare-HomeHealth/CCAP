using CCAP.Domain.Entities;
namespace CCAP.Application.Abstractions.Persistence;
public interface IPatientCareLogRepository{Task AddAsync(PatientCareLog log,CancellationToken cancellationToken);Task<List<PatientCareLog>> GetByPatientIdAsync(Guid patientId,CancellationToken cancellationToken);}
