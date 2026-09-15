using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Storage;
using CCAP.Infrastructure.Identity;
using CCAP.Infrastructure.Persistence;
using CCAP.Infrastructure.Persistence.Repositories;
using CCAP.Infrastructure.Storage;
using CCAP.Infrastructure.Storage.Local;
using CCAP.Infrastructure.Storage.AzureBlob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CCAP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "DefaultConnection")));

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        services.AddScoped<
            INotificationRepository,
            NotificationRepository>();

        services.AddScoped<
            INotificationReadStateRepository,
            NotificationReadStateRepository>();

        services.AddScoped<
            IAnnouncementRepository,
            AnnouncementRepository>();

        services.AddScoped<
            IDashboardRepository,
            DashboardRepository>();

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            IRoleRepository,
            RoleRepository>();

        services.AddScoped<
            IPatientRepository,
            PatientRepository>();

        services.AddScoped<
            IReferralRepository,
            ReferralRepository>();

        services.AddScoped<
            IReferralDraftRepository,
            ReferralDraftRepository>();

        services.AddScoped<
            ILocationRepository,
            LocationRepository>();

        services.AddScoped<
            IReferralDocumentRepository,
            ReferralDocumentRepository>();

        services.AddScoped<
            IServiceTypeRepository,
            ServiceTypeRepository>();

        services.AddScoped<
            IPatientTaskRepository,
            PatientTaskRepository>();

        services.AddScoped<
            IComplianceRepository,
            ComplianceRepository>();

        services.AddScoped<
            ICallNoteRepository,
            CallNoteRepository>();

        services.AddScoped<IPatientAuditLogRepository, PatientAuditLogRepository>();
        services.AddScoped<IPatientCareLogRepository, PatientCareLogRepository>();

        services.AddScoped<
            IUnitOfWork,
            UnitOfWork>();

        services.AddScoped<
            IAdminLookupRepository,
            AdminLookupRepository>();

        services.AddScoped<
            ILookupOptionRepository,
            LookupOptionRepository>();

        services.AddScoped<
            IPasswordHasher,
            PasswordHasherService>();

        services.AddScoped<
            IJwtService,
            JwtService>();

        // =========================================================
        // FILE STORAGE
        // =========================================================

        services.Configure<FileStorageOptions>(
            configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<AzureBlobOptions>(
            configuration.GetSection(AzureBlobOptions.SectionName));

        var storageProvider = configuration["FileStorage:Provider"]?.Trim();

        switch (storageProvider?.ToUpperInvariant())
        {
            case "AZUREBLOB":
            case "AZURE_BLOB":
                services.AddScoped<IFileStorage, AzureBlobFileStorage>();
                break;
            case "LOCAL":
                services.AddScoped<IFileStorage, LocalFileStorage>();
                break;
            case null:
            case "":
            case "NONE":
            case "METADATAONLY":
            default:
                services.AddScoped<IFileStorage, NullFileStorage>();
                break;
        }

        // =========================================================
        // END FILE STORAGE
        // =========================================================

        return services;
    }
}
