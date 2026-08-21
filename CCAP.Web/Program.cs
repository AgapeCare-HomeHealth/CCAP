using CCAP.Web.Components;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Authentication.State;
using CCAP.Web.Features.Dashboard.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Notifications.Services;
using CCAP.Web.Features.Tracker.PatientWorkflow.Services;
using CCAP.Web.Features.Tracker.ReferralIntake.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<TokenStore>();

builder.Services.AddScoped<CcapAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    serviceProvider =>
        serviceProvider.GetRequiredService<
            CcapAuthenticationStateProvider>());

builder.Services.AddSingleton(new MockDataOptions
{
    Enabled = builder.Configuration.GetValue<bool>("MockData:Enabled")
});
builder.Services.AddSingleton<MockDataStore>();

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CcapApiClient>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PatientWorkflowService>();
builder.Services.AddScoped<PatientCareManagementService>();
builder.Services.AddScoped<NotificationCenterService>();
builder.Services.AddScoped<ReferralIntakeService>();

builder.Services.AddScoped<
    CCAP.Web.Features.Admin.Users.Services.UserServices>();

builder.Services.AddScoped<
    CCAP.Web.Features.Admin.Users.Services.AdminLookupService>();

builder.Services.AddScoped<
    CCAP.Web.Features.Admin.Roles.Services.RoleService>();

builder.Services.AddScoped<
    CCAP.Web.Features.Patients.Services.PatientService>();

builder.Services.AddScoped<
    CCAP.Web.Features.Tracker.PatientWorkflow.Services.PatientClinicalService>();


var apiBaseUrl =
    builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Api:BaseUrl is not configured.");

builder.Services.AddHttpClient("CCAP.Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
