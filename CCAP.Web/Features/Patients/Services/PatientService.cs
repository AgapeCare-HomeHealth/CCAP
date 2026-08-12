using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Patients.Models;

namespace CCAP.Web.Features.Patients.Services;

public sealed class PatientService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public PatientService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<List<PatientListItem>> GetPatientsAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.Patients.ToList();

        return await _api.GetFromJsonAsync<List<PatientListItem>>("api/patients", cancellationToken) ?? [];
    }
}
