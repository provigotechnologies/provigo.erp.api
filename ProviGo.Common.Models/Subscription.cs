using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Subscriptions")]
    public class Subscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [Required]
        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }

}
