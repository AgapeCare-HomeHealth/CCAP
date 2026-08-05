using CCAP.Data.DTOs.Users;
using CCAP.Data.Interfaces;
using CCAP.Data.Persistence;
using CCAP.Data.Security;
using CCAP.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Data.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        private readonly PasswordHasherService _passwordHasher;

        public UserService(
            AppDbContext context,
            PasswordHasherService passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            return await _context.ApplicationUsers

                .Include(x => x.Role)

                .Include(x => x.Discipline)

                .Select(x => new UserResponse
                {
                    UserId = x.UserId,

                    EmployeeNo = x.EmployeeNo,

                    FirstName = x.FirstName,

                    LastName = x.LastName,

                    Email = x.Email,

                    MobileNo = x.MobileNo,

                    IsActive = x.IsActive,

                    Role = x.Role.RoleName,

                    Discipline = x.Discipline == null
                        ? ""
                        : x.Discipline.Name
                })

                .OrderBy(x => x.LastName)

                .ToListAsync();
        }

        public async Task<UserResponse?> GetByIdAsync(Guid id)
        {
            return await _context.ApplicationUsers

                .Include(x => x.Role)

                .Include(x => x.Discipline)

                .Where(x => x.UserId == id)

                .Select(x => new UserResponse
                {
                    UserId = x.UserId,

                    EmployeeNo = x.EmployeeNo,

                    FirstName = x.FirstName,

                    LastName = x.LastName,

                    Email = x.Email,

                    MobileNo = x.MobileNo,

                    IsActive = x.IsActive,

                    Role = x.Role.RoleName,

                    Discipline = x.Discipline != null
                        ? x.Discipline.Name
                        : ""
                })

                .FirstOrDefaultAsync();
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            // Check duplicate Employee Number
            if (await _context.ApplicationUsers
                .AnyAsync(x => x.EmployeeNo == request.EmployeeNo))
            {
                throw new Exception("Employee number already exists.");
            }

            // Check duplicate Email
            if (await _context.ApplicationUsers
                .AnyAsync(x => x.Email == request.Email))
            {
                throw new Exception("Email already exists.");
            }

            // Verify Role exists
            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId);

            if (role == null)
                throw new Exception("Invalid role.");

            // Verify Discipline exists (optional)
            if (request.DisciplineId.HasValue)
            {
                var disciplineExists = await _context.Disciplines
                    .AnyAsync(x => x.DisciplineId == request.DisciplineId.Value);

                if (!disciplineExists)
                    throw new Exception("Invalid discipline.");
            }

            var user = new ApplicationUser
            {
                UserId = Guid.NewGuid(),

                EmployeeNo = request.EmployeeNo,

                FirstName = request.FirstName,

                LastName = request.LastName,

                Email = request.Email,

                MobileNo = request.MobileNo,

                RoleId = request.RoleId,

                DisciplineId = request.DisciplineId,

                IsActive = true
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);

            _context.ApplicationUsers.Add(user);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(user.UserId)
                ?? throw new Exception("Unable to retrieve created user.");
        }

        public async Task UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                throw new Exception("User not found.");

            // Check duplicate employee number
            var employeeExists = await _context.ApplicationUsers
                .AnyAsync(x =>
                    x.EmployeeNo == request.EmployeeNo &&
                    x.UserId != id);

            if (employeeExists)
                throw new Exception("Employee number already exists.");

            // Check duplicate email
            var emailExists = await _context.ApplicationUsers
                .AnyAsync(x =>
                    x.Email == request.Email &&
                    x.UserId != id);

            if (emailExists)
                throw new Exception("Email already exists.");

            // Validate role
            var roleExists = await _context.Roles
                .AnyAsync(x => x.RoleId == request.RoleId);

            if (!roleExists)
                throw new Exception("Invalid role.");

            // Validate discipline (optional)
            if (request.DisciplineId.HasValue)
            {
                var disciplineExists = await _context.Disciplines
                    .AnyAsync(x =>
                        x.DisciplineId == request.DisciplineId.Value);

                if (!disciplineExists)
                    throw new Exception("Invalid discipline.");
            }

            user.EmployeeNo = request.EmployeeNo;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.MobileNo = request.MobileNo;
            user.RoleId = request.RoleId;
            user.DisciplineId = request.DisciplineId;
            user.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                throw new Exception("User not found.");

            user.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(Guid id)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                throw new Exception("User not found.");

            user.IsActive = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                throw new Exception("User not found.");

            user.IsActive = false;

            await _context.SaveChangesAsync();
        }
    }
}
