using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using CCAP.Web.Features.Authentication.Models;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class AuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly MockDataOptions _options;
    private readonly MockDataStore _mock;

    public AuthenticationService(IHttpClientFactory httpClientFactory, TokenStore tokenStore, MockDataOptions options, MockDataStore mock)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _options = options;
        _mock = mock;
    }

    public async Task<LoginResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return await MockLoginAsync(email, password);

        var client = _httpClientFactory.CreateClient("CCAP.Api");
        using var response = await client.PostAsJsonAsync("api/auth/login", new { email, password }, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: cancellationToken)
            ?? new LoginResultDto { Success = false, Message = "Invalid API response." };

        if (result.Success && !string.IsNullOrWhiteSpace(result.Token))
            await _tokenStore.SetAsync(result.Token);

        return result;
    }

    public Task LogoutAsync() => _tokenStore.DeleteAsync();

    public async Task RefreshMockAuthorizationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        var user = _mock.Users.FirstOrDefault(x => x.UserId == userId)
            ?? throw new InvalidOperationException("Mock user not found.");

        if (!user.IsActive)
        {
            await _tokenStore.DeleteAsync();
            return;
        }

        var token = CreateMockToken(user);
        await _tokenStore.SetAsync(token);
    }

    private async Task<LoginResultDto> MockLoginAsync(string email, string password)
    {
        var user = _mock.Users.FirstOrDefault(x =>
            string.Equals(x.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !user.IsActive || password != "Admin123!")
        {
            return new LoginResultDto { Success = false, Message = "Invalid email or password." };
        }

        var token = CreateMockToken(user);
        await _tokenStore.SetAsync(token);

        return new LoginResultDto
        {
            Success = true,
            Message = "Mock login successful.",
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(8),
            UserId = user.UserId,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    private string CreateMockToken(CCAP.Web.Features.Admin.Users.Models.UserDto user)
    {
        var permissionIds = _mock.RolePermissions.TryGetValue(user.RoleId, out var ids)
            ? ids
            : [];

        var permissions = _mock.Permissions
            .Where(x => permissionIds.Contains(x.PermissionId))
            .Select(x => x.PermissionCode)
            .ToArray();

        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.UserId.ToString(),
            ["name"] = user.FullName,
            ["email"] = user.Email,
            ["role"] = user.Role,
            ["permission"] = permissions,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()
        };

        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "none", typ = "JWT" }));
        var body = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        return $"{header}.{body}.mock";
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
