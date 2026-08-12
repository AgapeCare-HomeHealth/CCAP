using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Infrastructure.Identity;
using CCAP.Infrastructure.Persistence;
using CCAP.Infrastructure.Persistence.Repositories;
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
                configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IReferralRepository, ReferralRepository>();
        services.AddScoped<ICallNoteRepository, CallNoteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAdminLookupRepository, AdminLookupRepository>();
        services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
