using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Entities
{
    public class Role
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
            = new List<ApplicationUser>();

        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}
