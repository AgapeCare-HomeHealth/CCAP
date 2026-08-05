using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.DTOs.Users
{

    public class UserResponse
    {
        public Guid UserId { get; set; }

        public string EmployeeNo { get; set; } = "";

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string? MobileNo { get; set; }

        public bool IsActive { get; set; }

        public string Role { get; set; } = "";

        public string? Discipline { get; set; }
    }
}
