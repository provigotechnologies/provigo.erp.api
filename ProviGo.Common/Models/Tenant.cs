using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Tenant")]
    public class Tenant
    {
        [Key]
        public Guid TenantId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public string ConnectionString { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
