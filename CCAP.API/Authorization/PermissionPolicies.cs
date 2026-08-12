using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CCAP.API.Authorization;

public static class PermissionPolicies
{
    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";
    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";
    public const string PatientsView = "patients.view";
    public const string PatientsManage = "patients.manage";
    public const string ReferralsView = "referrals.view";
    public const string ReferralsManage = "referrals.manage";

    public static void AddCcapPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler,
            PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            Add(options, UsersView);
            Add(options, UsersManage);
            Add(options, RolesView);
            Add(options, RolesManage);
            Add(options, PatientsView);
            Add(options, PatientsManage);
            Add(options, ReferralsView);
            Add(options, ReferralsManage);
        });
    }

    private static void Add(
        AuthorizationOptions options, string permission) =>
        options.AddPolicy(permission, p =>
            p.Requirements.Add(new PermissionRequirement(permission)));
}
