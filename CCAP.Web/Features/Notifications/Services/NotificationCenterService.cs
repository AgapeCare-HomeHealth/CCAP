using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Notifications.Models;

namespace CCAP.Web.Features.Notifications.Services;

/// <summary>
/// Provides the global notification feed used by the topbar bell.
/// Mock mode calculates notifications from the same patient-care records used by the profile.
/// API mode calls GET /api/notifications. The Razor UI does not know which source is active.
/// </summary>
public sealed class NotificationCenterService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public NotificationCenterService(
        CcapApiClient api,
        MockDataStore mock,
        MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<List<UserNotificationDto>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.BuildGlobalNotifications();

        return await _api.GetFromJsonAsync<List<UserNotificationDto>>(
                   "api/notifications",
                   cancellationToken)
               ?? [];
    }

    public Task MarkReadAsync(
        UserNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            _mock.MarkGlobalNotificationRead(notification.NotificationId);
        }

        notification.IsRead = true;
        return Task.CompletedTask;
    }
}
