using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("Shifts")]
    public class Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShiftId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public Guid TrainerId { get; set; }

        [Required, MaxLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(TrainerId))]
        public User User { get; set; }
    }
}
