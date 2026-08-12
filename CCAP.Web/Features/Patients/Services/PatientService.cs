using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.Patients.Models;

namespace CCAP.Web.Features.Patients.Services;

public sealed class PatientService
{
    private readonly CcapApiClient _api;

    public PatientService(CcapApiClient api)
    {
        _api = api;
    }

    public async Task<List<PatientListItem>> GetPatientsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<PatientListItem>>(
            "api/patients",
            cancellationToken) ?? [];
    }
}