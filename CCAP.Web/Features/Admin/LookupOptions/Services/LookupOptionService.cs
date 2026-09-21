using CCAP.Web.Features.Admin.LookupOptions.Models;
using CCAP.Web.Features.Authentication.Services;
using System.Net.Http.Json;

namespace CCAP.Web.Features.Admin.LookupOptions.Services;

public sealed class LookupOptionService
{
    private readonly CcapApiClient _api;
    public LookupOptionService(CcapApiClient api) => _api = api;

    public async Task<List<LookupOptionDto>> GetAsync(string? type = null, bool activeOnly = false, CancellationToken cancellationToken = default)
        => await _api.GetFromJsonAsync<List<LookupOptionDto>>($"api/admin/lookup-options?type={Uri.EscapeDataString(type ?? string.Empty)}&activeOnly={activeOnly.ToString().ToLowerInvariant()}", cancellationToken) ?? [];

    public async Task CreateAsync(LookupOptionEditModel model, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync("api/admin/lookup-options", model, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(LookupOptionEditModel model, CancellationToken cancellationToken = default)
    {
        var response = await _api.PutAsJsonAsync($"api/admin/lookup-options/{model.LookupOptionId}", new { model.Code, model.DisplayName, model.SortOrder, model.IsActive }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _api.DeleteAsync($"api/admin/lookup-options/{id}", cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }
}
