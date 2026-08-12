using CCAP.Web.Features.Admin.Users.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.Users.Services;

public sealed class AdminLookupService
{
    private readonly CcapApiClient _api;

    public AdminLookupService(CcapApiClient api)
    {
        _api = api;
    }

    public async Task<List<LookupDto>> GetDisciplinesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<LookupDto>>(
            "api/admin/disciplines",
            cancellationToken) ?? [];
    }
}