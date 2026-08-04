using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Data.Entities
{
    public class Discipline
    {
        public Guid DisciplineId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
            = new List<ApplicationUser>();
    }
}
