using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.DTOs.Authentication
{
    public class LoginRequest
    {
        public string Email { get; set; } = "";

        public string Password { get; set; } = "";
    }
}
