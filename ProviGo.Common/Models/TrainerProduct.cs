using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("TrainerProducts")]
    public class TrainerProduct 
    {
        [Key]
        public int TrainerProductId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid BranchId { get; set; }
        public Guid TrainerId { get; set; }
        public int ProductId { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(TrainerId))]
        public User Trainer { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

    }
}