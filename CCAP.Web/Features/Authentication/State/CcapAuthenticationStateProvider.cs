using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Authentication.State;

public sealed class CcapAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokenStore;
    private bool _initialized;
    private string? _viewRole;

    public bool IsInitialized => _initialized;

    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    // This is a UI/testing role only. It never changes the ApplicationUser.RoleId
    // or the JWT stored for the real account.
    public string? ViewRole => _viewRole;

    public CcapAuthenticationStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetAsync();

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonymous);

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                await _tokenStore.DeleteAsync();
                _viewRole = null;
                return new AuthenticationState(Anonymous);
            }

            return new AuthenticationState(CreatePrincipalFromToken(token, _viewRole));
        }
        catch
        {
            await _tokenStore.DeleteAsync();
            _viewRole = null;
            return new AuthenticationState(Anonymous);
        }
    }

    public async Task LoadPersistedAsync()
    {
        if (_initialized)
            return;

        await _tokenStore.LoadPersistedAsync();
        _initialized = true;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SetViewRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return;

        var actualRole = await GetActualRoleAsync();
        if (!string.Equals(actualRole, "Administrator", StringComparison.OrdinalIgnoreCase))
            return;

        if (!IsSupportedViewRole(role))
            return;

        _viewRole = role;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        await Task.CompletedTask;
    }

    public async Task RestoreActualRoleAsync()
    {
        _viewRole = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        await Task.CompletedTask;
    }

    public async Task<string?> GetActualRoleAsync()
    {
        var token = await _tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c =>
                c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task NotifyLoginAsync()
    {
        _initialized = true;
        _viewRole = null;
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public async Task NotifyLogoutAsync()
    {
        await _tokenStore.DeleteAsync();
        _viewRole = null;
        _initialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static bool IsSupportedViewRole(string role) =>
        role.Equals("Care Coordinator", StringComparison.OrdinalIgnoreCase) ||
        role.Equals("Clinician", StringComparison.OrdinalIgnoreCase);

    private static ClaimsPrincipal CreatePrincipalFromToken(string token, string? viewRole)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var actualRole = jwt.Claims.FirstOrDefault(c =>
                c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;

            var effectiveRole = string.IsNullOrWhiteSpace(viewRole) ? actualRole : viewRole;
            var claims = new List<Claim>();

            foreach (var claim in jwt.Claims)
            {
                if (claim.Type == "role" || claim.Type == ClaimTypes.Role)
                    continue;

                if (claim.Type == "permission")
                    continue;

                if (claim.Type == "name")
                {
                    claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    continue;
                }

                if (claim.Type == "email")
                {
                    claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                    continue;
                }

                if (claim.Type == "sub")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                    continue;
                }

                claims.Add(claim);
            }

            if (!string.IsNullOrWhiteSpace(effectiveRole))
                claims.Add(new Claim(ClaimTypes.Role, effectiveRole));

            foreach (var permission in GetViewPermissions(effectiveRole, jwt))
                claims.Add(new Claim("permission", permission));

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "Bearer",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CCAP AUTH] Failed to parse JWT: {ex}");
            return Anonymous;
        }
    }

    private static IEnumerable<string> GetViewPermissions(string? role, JwtSecurityToken jwt)
    {
        if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase))
            return jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase);

        if (string.Equals(role, "Care Coordinator", StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                "dashboard.view", "users.view", "roles.view", "patients.view", "patients.manage",
                "referrals.view", "referrals.manage", "scheduling.view", "scheduling.manage",
                "fax.view", "fax.manage", "notifications.view", "notifications.manage",
                "notes.view", "notes.manage", "labs.view", "labs.manage", "supplies.view", "supplies.manage",
                "foley.view", "foley.manage", "orders.view", "orders.manage", "lookups.view"
            };
        }

        if (string.Equals(role, "Clinician", StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                "dashboard.view", "patients.view", "patients.manage", "referrals.view", "scheduling.view",
                "fax.view", "fax.manage", "notifications.view", "notifications.manage",
                "notes.view", "notes.manage", "labs.view", "labs.manage", "supplies.view", "supplies.manage",
                "foley.view", "foley.manage", "orders.view", "orders.manage"
            };
        }

        return Array.Empty<string>();
    }
}
