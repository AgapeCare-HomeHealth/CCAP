using CCAP.Application.Common.Validation;
using CCAP.Application.Features.Admin.Commands.CreateRole;
using CCAP.Application.Features.Admin.Commands.DeleteRole;
using CCAP.Application.Features.Admin.Commands.SetRolePermissions;
using CCAP.Application.Features.Admin.Commands.UpdateRole;
using CCAP.Application.Features.Admin.Queries.GetRolePermissions;
using CCAP.Application.Features.Announcements.Commands.CreateAnnouncement;
using CCAP.Application.Features.Authentication.Commands.Login;
using CCAP.Application.Features.Dashboard.Queries.GetDashboard;
using CCAP.Application.Features.Notifications.Queries.GetNotifications;
using CCAP.Application.Features.Patients.Commands.AddCallNote;
using CCAP.Application.Features.Patients.Commands.ArchivePatient;
using CCAP.Application.Features.Patients.Commands.CompleteCare;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using CCAP.Application.Features.Users.Commands.ActivateUser;
using CCAP.Application.Features.Users.Commands.CreateUser;
using CCAP.Application.Features.Users.Commands.DeactivateUser;
using CCAP.Application.Features.Users.Commands.DeleteUser;
using CCAP.Application.Features.Users.Commands.UpdateUser;
using CCAP.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CCAP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(DependencyInjection).Assembly));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(RequestValidationBehavior<,>));

        // =========================================================
        // ANNOUNCEMENTS
        // =========================================================

        services.AddTransient<
            IRequestValidator<CreateAnnouncementCommand>,
            CreateAnnouncementCommandValidator>();

        // =========================================================
        // NOTIFICATIONS
        // =========================================================

        services.AddTransient<
            IRequestValidator<GetNotificationsQuery>,
            GetNotificationsQueryValidator>();

        // =========================================================
        // ADMIN
        // =========================================================

        services.AddTransient<
            IRequestValidator<CreateRoleCommand>,
            CreateRoleCommandValidator>();

        services.AddTransient<
            IRequestValidator<UpdateRoleCommand>,
            UpdateRoleCommandValidator>();

        services.AddTransient<
            IRequestValidator<SetRolePermissionsCommand>,
            SetRolePermissionsCommandValidator>();

        services.AddTransient<
            IRequestValidator<DeleteRoleCommand>,
            DeleteRoleCommandValidator>();

        services.AddTransient<
            IRequestValidator<GetRolePermissionsQuery>,
            GetRolePermissionsQueryValidator>();

        // =========================================================
        // USERS
        // =========================================================

        services.AddTransient<
            IRequestValidator<CreateUserCommand>,
            CreateUserCommandValidator>();

        services.AddTransient<
            IRequestValidator<UpdateUserCommand>,
            UpdateUserCommandValidator>();

        services.AddTransient<
            IRequestValidator<DeleteUserCommand>,
            DeleteUserCommandValidator>();

        services.AddTransient<
            IRequestValidator<ActivateUserCommand>,
            ActivateUserCommandValidator>();

        services.AddTransient<
            IRequestValidator<DeactivateUserCommand>,
            DeactivateUserCommandValidator>();

        services.AddTransient<
            IRequestValidator<GetUserByIdQuery>,
            GetUserByIdQueryValidator>();

        // =========================================================
        // PATIENTS
        // =========================================================

        services.AddTransient<
            IRequestValidator<AddCallNoteCommand>,
            AddCallNoteCommandValidator>();

        services.AddTransient<
            IRequestValidator<ArchivePatientCommand>,
            ArchivePatientCommandValidator>();

        services.AddTransient<
            IRequestValidator<CompleteCareCommand>,
            CompleteCareCommandValidator>();

        // =========================================================
        // AUTHENTICATION
        // =========================================================

        services.AddTransient<
            IRequestValidator<LoginCommand>,
            LoginCommandValidator>();

        // =========================================================
        // REFERRALS
        // =========================================================

        services.AddTransient<
            IRequestValidator<CreateReferralIntakeCommand>,
            CreateReferralIntakeValidator>();

        // =========================================================
        // DASHBOARD
        // =========================================================

        services.AddTransient<
            IRequestValidator<GetDashboardQuery>,
            GetDashboardQueryValidator>();


        return services;
    }
}