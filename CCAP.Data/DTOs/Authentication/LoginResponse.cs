using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.DTOs.Authentication
{
    public class LoginResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public string Token { get; set; } = "";

        public DateTime Expiration { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = "";

        public string Role { get; set; } = "";
    }
}
