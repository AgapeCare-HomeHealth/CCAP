using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CCAP.Web.Features.Authentication.Models;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class AuthenticationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly MockDataOptions _options;
    private readonly MockDataStore _mock;

    public AuthenticationService(
        IHttpClientFactory httpClientFactory,
        TokenStore tokenStore,
        MockDataOptions options,
        MockDataStore mock)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _options = options;
        _mock = mock;
    }

    public async Task<LoginResultDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            return await MockLoginAsync(email, password);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("CCAP.Api");

            using var response = await client.PostAsJsonAsync(
                "api/auth/login",
                new
                {
                    email = email.Trim(),
                    password
                },
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = GetFailureMessage(response.StatusCode, responseBody)
                };
            }

            var result = JsonSerializer.Deserialize<LoginResultDto>(
                responseBody,
                JsonOptions);

            if (result is null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = "The login service returned an invalid response. Please try again."
                };
            }

            if (result.Success && !string.IsNullOrWhiteSpace(result.Token))
            {
                await _tokenStore.SetAsync(result.Token);
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "The login request timed out. Please try again."
            };
        }
        catch (HttpRequestException)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "Unable to reach the CCAP API. Please make sure the API is running and try again."
            };
        }
        catch (JsonException)
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "The login service returned an invalid response. Please try again."
            };
        }
    }

    public Task LogoutAsync() => _tokenStore.DeleteAsync();

    public async Task RefreshMockAuthorizationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
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

    private async Task<LoginResultDto> MockLoginAsync(
        string email,
        string password)
    {
        var user = _mock.Users.FirstOrDefault(x =>
            string.Equals(
                x.Email,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (user is null || !user.IsActive || password != "Admin123!")
        {
            return new LoginResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
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

    private static string GetFailureMessage(
        HttpStatusCode statusCode,
        string responseBody)
    {
        var apiMessage = TryReadApiMessage(responseBody);

        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                apiMessage ?? "Please check the login details and try again.",

            HttpStatusCode.Unauthorized =>
                apiMessage ?? "Invalid email or password.",

            HttpStatusCode.Forbidden =>
                "Your account is not allowed to access CCAP.",

            HttpStatusCode.TooManyRequests =>
                "Too many login attempts. Please wait a moment and try again.",

            _ when (int)statusCode >= 500 =>
                "The CCAP API is temporarily unavailable. Please try again.",

            _ =>
                apiMessage ?? "Unable to sign in. Please try again."
        };
    }

    private static string? TryReadApiMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                if (errors.ValueKind == JsonValueKind.Array)
                {
                    foreach (var error in errors.EnumerateArray())
                    {
                        if (error.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(error.GetString()))
                        {
                            return error.GetString();
                        }
                    }
                }

                if (errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in errors.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var error in property.Value.EnumerateArray())
                            {
                                if (error.ValueKind == JsonValueKind.String &&
                                    !string.IsNullOrWhiteSpace(error.GetString()))
                                {
                                    return error.GetString();
                                }
                            }
                        }
                    }
                }
            }

            if (root.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(message.GetString()))
            {
                return message.GetString();
            }

            if (root.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return title.GetString();
            }
        }
        catch (JsonException)
        {
            // The UI should never display raw server response bodies.
        }

        return null;
    }

    private string CreateMockToken(
        CCAP.Web.Features.Admin.Users.Models.UserDto user)
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

        var header = Base64Url(
            JsonSerializer.SerializeToUtf8Bytes(
                new { alg = "none", typ = "JWT" }));

        var body = Base64Url(
            JsonSerializer.SerializeToUtf8Bytes(payload));

        return $"{header}.{body}.mock";
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
