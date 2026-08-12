using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientClinicalService
{
    private readonly CcapApiClient _api;

    public PatientClinicalService(CcapApiClient api)
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
}