using CCAP.Web.Features.Admin.Users.Models;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Admin.Users.Services;

public sealed class AdminLookupService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public AdminLookupService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<List<LookupDto>> GetDisciplinesAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.Disciplines.Select(x => new LookupDto { DisciplineId = x.DisciplineId, Code = x.Code, Name = x.Name }).ToList();

        return await _api.GetFromJsonAsync<List<LookupDto>>("api/admin/disciplines", cancellationToken) ?? [];
    }
}
