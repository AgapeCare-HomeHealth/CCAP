using System.Net.Http.Json;
using CCAP.Web.Features.Admin.ServiceTypes.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.ServiceTypes.Services;

public sealed class ServiceTypeAdminService
{
    private readonly CcapApiClient _api;
    public ServiceTypeAdminService(CcapApiClient api) => _api = api;

    public async Task<List<ServiceTypeDto>> GetAsync(bool activeOnly = false, CancellationToken cancellationToken = default) =>
        await _api.GetFromJsonAsync<List<ServiceTypeDto>>($"api/admin/service-types?activeOnly={activeOnly.ToString().ToLowerInvariant()}", cancellationToken) ?? [];

    public async Task CreateAsync(ServiceTypeEditModel model, CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync("api/admin/service-types", model, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(ServiceTypeEditModel model, CancellationToken cancellationToken = default)
    {
        var response = await _api.PutAsJsonAsync($"api/admin/service-types/{model.ServiceTypeId}", new { model.Code, model.Name, model.Icon, model.CssClass, model.IsActive }, cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _api.DeleteAsync($"api/admin/service-types/{id}", cancellationToken);
        await _api.EnsureSuccessAsync(response, cancellationToken);
    }
}
