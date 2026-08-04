using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Entities
{

    public class Permission
    {
        public Guid PermissionId { get; set; }

        public string PermissionCode { get; set; } = string.Empty;

        public string PermissionName { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}
