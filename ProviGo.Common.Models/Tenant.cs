using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
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
