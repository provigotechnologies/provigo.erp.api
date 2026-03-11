using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Branches")]
    public class Branch
    {
        [Key]
        public Guid BranchId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TenantId { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }

        [Required]
        [MaxLength(150)]
        public string BranchName { get; set; }

        [MaxLength(10)]
        public string BranchCode { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public string? Address { get; set; }

        [MaxLength(15)]
        public string GSTIN { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        [ForeignKey(nameof(CountryId))]
        public Country Country { get; set; }

        [ForeignKey(nameof(StateId))]
        public State State { get; set; }

    }
}
