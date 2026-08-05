using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Entities
{

    public class ApplicationUser
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public Guid? DisciplineId { get; set; }

        public string EmployeeNo { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? MobileNo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Role Role { get; set; } = null!;

        public Discipline? Discipline { get; set; }
    }
}
