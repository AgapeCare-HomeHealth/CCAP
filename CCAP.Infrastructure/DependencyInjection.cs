using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Storage;
using CCAP.Infrastructure.Identity;
using CCAP.Infrastructure.Persistence;
using CCAP.Infrastructure.Persistence.Repositories;
using CCAP.Infrastructure.Storage;
using CCAP.Infrastructure.Storage.Local;
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

        services.AddScoped<
            IUnitOfWork,
            UnitOfWork>();

        services.AddScoped<
            IAdminLookupRepository,
            AdminLookupRepository>();

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
            configuration.GetSection(
                FileStorageOptions.SectionName));

        services.AddScoped<IFileStorage, LocalFileStorage>();

        // =========================================================
        // END FILE STORAGE
        // =========================================================

        return services;
    }
}
