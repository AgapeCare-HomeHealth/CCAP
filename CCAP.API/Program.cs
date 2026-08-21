using CCAP.API.Authorization;
using CCAP.Application;
using CCAP.Infrastructure;
using CCAP.Infrastructure.Identity;
using CCAP.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey =
            builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key is not configured.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"]
                    ?? throw new InvalidOperationException(
                        "Jwt:Issuer is not configured."),

                ValidateAudience = true,

                ValidAudience =
                    builder.Configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException(
                        "Jwt:Audience is not configured."),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddCcapPolicies();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CCAP.Infrastructure.Persistence.AppDbContext>();

    // In development you may set Database:ApplyMigrations=true to apply
    // committed EF Core migrations automatically. Production deployments can
    // instead apply the generated migration SQL as a deployment step.
    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
    {
        await context.Database.MigrateAsync();
    }

    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<CCAP.Application.Abstractions.Identity.IPasswordHasher>();

    await DatabaseSeeder.SeedAsync(context, passwordHasher);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
