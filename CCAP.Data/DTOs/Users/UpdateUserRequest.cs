using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string EmployeeNo { get; set; } = "";

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string? MobileNo { get; set; }

        public Guid RoleId { get; set; }

        public Guid? DisciplineId { get; set; }

        public bool IsActive { get; set; }
    }
}
