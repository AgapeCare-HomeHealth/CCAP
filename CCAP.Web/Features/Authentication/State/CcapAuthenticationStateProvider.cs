using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Authentication.State;

public sealed class CcapAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;

    private bool _initialized;

    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    public CcapAuthenticationStateProvider(
        TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public bool IsInitialized => _initialized;

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        var principal = CreatePrincipalFromToken(token);

        return new AuthenticationState(principal);
    }

    public async Task LoadPersistedAsync()
    {
        if (_initialized)
            return;

        await _tokenStore.LoadPersistedAsync();

        _initialized = true;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());
    }

    public async Task NotifyLoginAsync()
    {
        _initialized = true;

        NotifyAuthenticationStateChanged(
            GetAuthenticationStateAsync());

        await Task.CompletedTask;
    }

    public async Task NotifyLogoutAsync()
    {
        await _tokenStore.DeleteAsync();

        _initialized = true;

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipalFromToken(
        string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim>();

            foreach (var claim in jwt.Claims)
            {
                if (claim.Type == "role" ||
                    claim.Type == ClaimTypes.Role)
                {
                    claims.Add(
                        new Claim(
                            ClaimTypes.Role,
                            claim.Value));

                    continue;
                }

                if (claim.Type == "permission")
                {
                    claims.Add(
                        new Claim(
                            "permission",
                            claim.Value));

                    continue;
                }

                claims.Add(claim);
            }

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "Bearer",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return Anonymous;
        }
    }
}