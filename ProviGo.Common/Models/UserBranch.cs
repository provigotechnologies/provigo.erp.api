using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    [Table("UserBranches")]
    public class UserBranch
    {
        [Key]
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
