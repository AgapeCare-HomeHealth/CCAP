using System.Net.Http.Json;
using CCAP.Web.Features.Authentication.Models;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class AuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;

    public AuthenticationService(
        IHttpClientFactory httpClientFactory,
        TokenStore tokenStore)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
    }

    public async Task<LoginResultDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("CCAP.Api");
        using var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email, password },
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginResultDto>(
            cancellationToken: cancellationToken)
            ?? new LoginResultDto { Success = false, Message = "Invalid API response." };

        if (result.Success && !string.IsNullOrWhiteSpace(result.Token))
            await _tokenStore.SetAsync(result.Token);

        return result;
    }

    public Task LogoutAsync() => _tokenStore.DeleteAsync();
}
