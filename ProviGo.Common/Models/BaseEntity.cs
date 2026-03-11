using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    public abstract class BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }

        public bool IsActive { get; set; }

    }
}
