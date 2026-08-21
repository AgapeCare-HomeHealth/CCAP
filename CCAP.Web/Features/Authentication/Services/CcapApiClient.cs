using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CCAP.Web.Features.Authentication.Services;

public sealed class CcapApiClient
{
    private readonly HttpClient _client;
    private readonly TokenStore _tokenStore;

    public CcapApiClient(
        IHttpClientFactory httpClientFactory,
        TokenStore tokenStore)
    {
        _client = httpClientFactory.CreateClient("CCAP.Api");
        _tokenStore = tokenStore;
    }

    private async Task AddAuthorizationAsync(
        HttpRequestMessage request)
    {
        var token = await _tokenStore.GetAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

        await AddAuthorizationAsync(request);

        return await _client.SendAsync(
            request,
            cancellationToken);
    }

    public async Task<T?> GetFromJsonAsync<T>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

        await AddAuthorizationAsync(request);

        using var response =
            await _client.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        //return await response.Content
        //    .ReadFromJsonAsync<T>(
        //        cancellationToken: cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API request failed. " +
                $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                $"Endpoint: {requestUri}. " +
                $"Response: {body}");
        }

        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonSerializer.Deserialize<T>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

        request.Content =
            JsonContent.Create(value);

        await AddAuthorizationAsync(request);

        return await _client.SendAsync(
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                requestUri);

        request.Content =
            JsonContent.Create(value);

        await AddAuthorizationAsync(request);

        return await _client.SendAsync(
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> PatchAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                requestUri);

        await AddAuthorizationAsync(request);

        return await _client.SendAsync(
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                requestUri);

        await AddAuthorizationAsync(request);

        return await _client.SendAsync(
            request,
            cancellationToken);
    }

    public async Task<HttpResponseMessage> PostMultipartAsync(
        string requestUri,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

        request.Content = content;

        await AddAuthorizationAsync(request);

        var fullUrl =
        new Uri(
            _client.BaseAddress!,
            requestUri);

        Debug.WriteLine(
            $"CCAP API REQUEST: POST {fullUrl}");

        return await _client.SendAsync(
            request,
            cancellationToken);
    }
}