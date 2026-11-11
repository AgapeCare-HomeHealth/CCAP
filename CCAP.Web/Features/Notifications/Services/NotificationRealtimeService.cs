using Microsoft.AspNetCore.SignalR.Client;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Notifications.Services;

public sealed class NotificationRealtimeService : IAsyncDisposable
{
    private readonly TokenStore _tokens;
    private readonly string _apiBaseUrl;
    private HubConnection? _connection;
    private readonly SemaphoreSlim _gate = new(1, 1);
    public event Action? NotificationReceived;
    public NotificationRealtimeService(TokenStore tokens, IConfiguration configuration)
    {
        _tokens = tokens;
        _apiBaseUrl = configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
    }
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.State == HubConnectionState.Connected) return;
            if (_connection is null)
            {
                var hubUri = new Uri(new Uri(_apiBaseUrl), "/hubs/notifications");
                _connection = new HubConnectionBuilder()
                    .WithUrl(hubUri, options =>
                    {
                        options.AccessTokenProvider = async () => await _tokens.GetAsync();
                    })
                    .WithAutomaticReconnect()
                    .Build();
                _connection.On<Guid>("NotificationCreated", _ => NotificationReceived?.Invoke());
            }
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync(cancellationToken);
        }
        finally { _gate.Release(); }
    }
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        _gate.Dispose();
    }
}
