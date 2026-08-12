using CCAP.Web.Features.Admin.Users.Models;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Admin.Users.Services;

public sealed class UserServices
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public UserServices(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) return _mock.Users.Select(Clone).ToList();
        return await _api.GetFromJsonAsync<List<UserDto>>("api/users", cancellationToken) ?? [];
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) return _mock.Users.Where(x => x.UserId == userId).Select(Clone).FirstOrDefault();
        return await _api.GetFromJsonAsync<UserDto>($"api/users/{userId}", cancellationToken);
    }

    public async Task<UserDto?> CreateUserAsync(UserEditModel model, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == model.RoleId);
            var discipline = model.DisciplineId.HasValue ? _mock.Disciplines.FirstOrDefault(x => x.DisciplineId == model.DisciplineId.Value) : null;
            var user = new UserDto
            {
                UserId = Guid.NewGuid(), EmployeeNo = model.EmployeeNo, FirstName = model.FirstName, LastName = model.LastName,
                Email = model.Email, MobileNo = model.MobileNo, RoleId = model.RoleId, DisciplineId = model.DisciplineId,
                Role = role?.RoleName ?? "", Discipline = discipline?.Name ?? "", IsActive = model.IsActive, LastLoginAt = null
            };
            _mock.Users.Add(user);
            if (role is not null) role.UserCount++;
            return Clone(user);
        }

        var response = await _api.PostAsJsonAsync("api/users", new { model.EmployeeNo, model.FirstName, model.LastName, model.Email, Password = model.Password, model.MobileNo, model.RoleId, model.DisciplineId }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);
    }

    public async Task UpdateUserAsync(UserEditModel model, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            var user = _mock.Users.FirstOrDefault(x => x.UserId == model.UserId) ?? throw new InvalidOperationException("Mock user not found.");
            user.EmployeeNo = model.EmployeeNo; user.FirstName = model.FirstName; user.LastName = model.LastName; user.Email = model.Email;
            user.MobileNo = model.MobileNo; user.RoleId = model.RoleId; user.DisciplineId = model.DisciplineId; user.IsActive = model.IsActive;
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == model.RoleId); user.Role = role?.RoleName ?? "";
            var discipline = model.DisciplineId.HasValue ? _mock.Disciplines.FirstOrDefault(x => x.DisciplineId == model.DisciplineId.Value) : null; user.Discipline = discipline?.Name ?? "";
            return;
        }
        var response = await _api.PutAsJsonAsync($"api/users/{model.UserId}", new { model.UserId, model.EmployeeNo, model.FirstName, model.LastName, model.Email, model.MobileNo, model.RoleId, model.DisciplineId }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) { SetActive(userId, true); return; }
        var response = await _api.PatchAsync($"api/users/{userId}/activate", cancellationToken); response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) { SetActive(userId, false); return; }
        var response = await _api.PatchAsync($"api/users/{userId}/deactivate", cancellationToken); response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled) { _mock.Users.RemoveAll(x => x.UserId == userId); return; }
        var response = await _api.DeleteAsync($"api/users/{userId}", cancellationToken); response.EnsureSuccessStatusCode();
    }

    private void SetActive(Guid userId, bool active)
    {
        var user = _mock.Users.FirstOrDefault(x => x.UserId == userId);
        if (user is not null) user.IsActive = active;
    }

    private static UserDto Clone(UserDto x) => new() { UserId = x.UserId, EmployeeNo = x.EmployeeNo, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email, MobileNo = x.MobileNo, RoleId = x.RoleId, DisciplineId = x.DisciplineId, Role = x.Role, Discipline = x.Discipline, IsActive = x.IsActive, LastLoginAt = x.LastLoginAt };
}
