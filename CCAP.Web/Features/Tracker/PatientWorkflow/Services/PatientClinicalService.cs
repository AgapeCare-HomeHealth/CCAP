using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientClinicalService
{
    private readonly CcapApiClient _api;

    public PatientClinicalService(
        CcapApiClient api)
    {
        _api = api;
    }

    public async Task<List<ServiceTypeDto>> GetServiceTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<ServiceTypeDto>>(
            "api/patients/service-types",
            cancellationToken) ?? [];
    }

    public async Task ScheduleSocAsync(
        Guid patientId,
        DateOnly socDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/soc/schedule",
            new { SocDate = socDate },
            cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CompleteSocAsync(
    Guid patientId,
    string? notes = null,
    CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            $"api/patients/{patientId}/soc/complete",
            new
            {
                Notes = notes
            },
            cancellationToken);

        await _api.EnsureSuccessAsync(response, cancellationToken);
    }
}