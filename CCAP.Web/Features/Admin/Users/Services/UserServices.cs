using System.Net.Http.Json;
using CCAP.Web.Features.Admin.Users.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.Users.Services;

public sealed class UserServices
{
    private readonly CcapApiClient _api;

    public UserServices(CcapApiClient api)
    {
        _api = api;
    }

    public async Task<List<UserDto>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<UserDto>>(
            "api/users",
            cancellationToken) ?? [];
    }

    public async Task<UserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<UserDto>(
            $"api/users/{userId}",
            cancellationToken);
    }

    public async Task<UserDto?> CreateUserAsync(
        UserEditModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PostAsJsonAsync(
            "api/users",
            new
            {
                model.EmployeeNo,
                model.FirstName,
                model.LastName,
                model.Email,
                Password = model.Password,
                model.MobileNo,
                model.RoleId,
                model.DisciplineId
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<UserDto>(
                cancellationToken: cancellationToken);
    }

    public async Task UpdateUserAsync(
        UserEditModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PutAsJsonAsync(
            $"api/users/{model.UserId}",
            new
            {
                model.UserId,
                model.EmployeeNo,
                model.FirstName,
                model.LastName,
                model.Email,
                model.MobileNo,
                model.RoleId,
                model.DisciplineId
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PatchAsync(
            $"api/users/{userId}/activate",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PatchAsync(
            $"api/users/{userId}/deactivate",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.DeleteAsync(
            $"api/users/{userId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}