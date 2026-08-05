using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CCAP.Data.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required]
        public string EmployeeNo { get; set; } = "";

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = "";

        public string? MobileNo { get; set; }

        public Guid RoleId { get; set; }

        public Guid? DisciplineId { get; set; }
    }
}
