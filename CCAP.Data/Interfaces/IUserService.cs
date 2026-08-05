using CCAP.Data.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> GetByIdAsync(Guid id);

        Task<UserResponse> CreateAsync(CreateUserRequest request);

        Task UpdateAsync(Guid id, UpdateUserRequest request);

        Task DeleteAsync(Guid id);

        Task ActivateAsync(Guid id);

        Task DeactivateAsync(Guid id);
    }
}
