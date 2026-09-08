using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientCareManagementService
{
    private readonly CcapApiClient _api;

    public PatientCareManagementService(
        CcapApiClient api)
    {
        _api = api;
    }

    public async Task<PatientCareProfileDto?> GetAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<PatientCareProfileDto>(
            $"api/patients/{patientId}/care-management",
            cancellationToken);
    }
}