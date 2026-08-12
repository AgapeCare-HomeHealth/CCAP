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
    public const string FaxView = "fax.view";
    public const string FaxManage = "fax.manage";
    public const string NotificationsView = "notifications.view";
    public const string NotificationsManage = "notifications.manage";
    public const string NotesView = "notes.view";
    public const string NotesManage = "notes.manage";
    public const string LabsView = "labs.view";
    public const string LabsManage = "labs.manage";
    public const string SuppliesView = "supplies.view";
    public const string SuppliesManage = "supplies.manage";
    public const string FoleyView = "foley.view";
    public const string FoleyManage = "foley.manage";
    public const string OrdersView = "orders.view";
    public const string OrdersManage = "orders.manage";

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
            Add(options, FaxView);
            Add(options, FaxManage);
            Add(options, NotificationsView);
            Add(options, NotificationsManage);
            Add(options, NotesView);
            Add(options, NotesManage);
            Add(options, LabsView);
            Add(options, LabsManage);
            Add(options, SuppliesView);
            Add(options, SuppliesManage);
            Add(options, FoleyView);
            Add(options, FoleyManage);
            Add(options, OrdersView);
            Add(options, OrdersManage);
        });
    }

    private static void Add(
        AuthorizationOptions options, string permission) =>
        options.AddPolicy(permission, p =>
            p.Requirements.Add(new PermissionRequirement(permission)));
}
