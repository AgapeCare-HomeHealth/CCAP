using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class TokenStore
{
    private const string StorageKey = "ccap.auth.token";

    private readonly ProtectedLocalStorage _storage;

    private string? _token;

    public TokenStore(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public Task<string?> GetAsync()
    {
        // Never access ProtectedLocalStorage here.
        // GetAsync can be called while authentication state is being evaluated.
        return Task.FromResult(_token);
    }

    public async Task SetAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException(
                "Authentication token cannot be empty.",
                nameof(token));

        _token = token;

        await _storage.SetAsync(
            StorageKey,
            token);
    }

    public async Task LoadPersistedAsync()
    {
        var result = await _storage.GetAsync<string>(StorageKey);

        if (result.Success &&
            !string.IsNullOrWhiteSpace(result.Value))
        {
            _token = result.Value;
        }
        else
        {
            _token = null;
        }
    }

    public async Task DeleteAsync()
    {
        _token = null;

        await _storage.DeleteAsync(StorageKey);
    }

    public bool HasToken =>
        !string.IsNullOrWhiteSpace(_token);
}