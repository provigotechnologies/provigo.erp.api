using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Licenses")]
    public class License
    {
        [Key]
        public int LicenseId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [Required, MaxLength(100)]
        public string LicenseKey { get; set; } = string.Empty;

        [Required]
        public DateTime ActivatedOn { get; set; }

        [Required]
        public DateTime SupportExpiry { get; set; }

        public bool IsActive { get; set; } = true;

    }

}
