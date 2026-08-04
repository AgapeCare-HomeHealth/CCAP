using CCAP.Data.DTOs.Authentication;
using CCAP.Data.Interfaces;
using CCAP.Data.Persistence;
using CCAP.Data.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;

        private readonly PasswordHasherService _passwordHasher;

        private readonly IJwtService _jwtService;

        public AuthenticationService(
            AppDbContext context,
            PasswordHasherService passwordHasher,
            IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.ApplicationUsers
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var validPassword = _passwordHasher.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (!validPassword)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponse
            {
                Success = true,

                Message = "Login successful.",

                Token = token,

                Expiration = DateTime.UtcNow.AddMinutes(60),

                UserId = user.UserId,

                FullName = $"{user.FirstName} {user.LastName}",

                Role = user.Role.RoleName
            };
        }
    }
}
