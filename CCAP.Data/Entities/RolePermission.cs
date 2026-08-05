using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Entities
{
    public class RolePermission
    {
        public Guid RolePermissionId { get; set; }

        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }

        public Role Role { get; set; } = null!;

        public Permission Permission { get; set; } = null!;
    }
}
