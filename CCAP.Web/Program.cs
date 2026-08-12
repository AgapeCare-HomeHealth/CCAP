using CCAP.Web.Components;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Authentication.State;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using CCAP.Web.Features.Authentication.Services;

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

builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddScoped<CcapApiClient>();

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

builder.Services.AddScoped<CcapApiClient>();

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