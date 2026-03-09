using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        // Multi-tenant
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        public int OrderId { get; set; }

        // Invoice Info
        [Required, MaxLength(30)]
        public string InvoiceNumber { get; set; } = "";

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        // ===============================
        // COMPANY SNAPSHOT (Freeze)
        // ===============================
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(500)]
        public string CompanyAddress { get; set; }

        [MaxLength(20)]
        public string CompanyGSTIN { get; set; }

        [MaxLength(20)]
        public string CompanyPhone { get; set; }

        // ===============================
        // BRANCH SNAPSHOT
        // ===============================
        [MaxLength(200)]
        public string BranchName { get; set; }

        [MaxLength(500)]
        public string BranchAddress { get; set; }

        [MaxLength(20)]
        public string BranchGSTIN { get; set; }

        // ===============================
        // CUSTOMER SNAPSHOT
        // ===============================
        [MaxLength(200)]
        public string CustomerName { get; set; }

        [MaxLength(20)]
        public string CustomerPhone { get; set; }

        [MaxLength(500)]
        public string CustomerAddress { get; set; }

        [MaxLength(20)]
        public string CustomerGSTIN { get; set; }

        // ===============================
        // TAX INFO
        // ===============================
        public bool IsGST { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal RoundOffAmount { get; set; }
        public decimal GrandTotal { get; set; }

        // ===============================
        // PAYMENT INFO
        // ===============================
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";
        // Draft | Issued | PartiallyPaid | Paid | Cancelled

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch Branch { get; set; }

        [ForeignKey(nameof(TenantId))]
        public TenantDetails TenantDetails { get; set; }

        // Navigation
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}