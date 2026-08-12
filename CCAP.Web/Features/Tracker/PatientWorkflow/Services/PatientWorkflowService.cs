using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Tracker.PatientWorkflow.Model;
using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Services;

public sealed class PatientWorkflowService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public PatientWorkflowService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<PatientWorkflowDto?> GetAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.GetPatientWorkflow(patientId);

        return await _api.GetFromJsonAsync<PatientWorkflowDto>(
            $"api/patients/{patientId}/workflow", cancellationToken);
    }
    public async Task UpdateHeaderAsync(PatientEditModel model, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            _mock.UpdatePatient(model);
            return;
        }

        var response = await _api.PutAsJsonAsync(
            $"api/patients/{model.PatientId}",
            model,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

}
