using System.Net.Http.Headers;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class ApiAuthenticationHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly ILogger<ApiAuthenticationHandler> _logger;

    public ApiAuthenticationHandler(
        TokenStore tokenStore,
        ILogger<ApiAuthenticationHandler> logger)
    {
        _tokenStore = tokenStore;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            _logger.LogDebug(
                "Bearer token attached to API request {Method} {Uri}.",
                request.Method,
                request.RequestUri);
        }
        else
        {
            _logger.LogWarning(
                "No authentication token available for API request {Method} {Uri}.",
                request.Method,
                request.RequestUri);
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}