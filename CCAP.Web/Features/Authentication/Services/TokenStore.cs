using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;

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
        return Task.FromResult(_token);
    }

    public async Task SetAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(
                "Authentication token cannot be empty.",
                nameof(token));
        }

        _token = token;

        await _storage.SetAsync(
            StorageKey,
            token);
    }

    public async Task LoadPersistedAsync()
    {
        try
        {
            var result =
                await _storage.GetAsync<string>(StorageKey);

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
        catch (CryptographicException)
        {
            // The browser contains a value encrypted with an
            // old/different ASP.NET Core Data Protection key.
            _token = null;

            try
            {
                await _storage.DeleteAsync(StorageKey);
            }
            catch
            {
                // Nothing else should prevent the application
                // from starting as unauthenticated.
            }
        }
        catch (Exception)
        {
            _token = null;

            try
            {
                await _storage.DeleteAsync(StorageKey);
            }
            catch
            {
                // Ignore storage cleanup failures.
            }
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