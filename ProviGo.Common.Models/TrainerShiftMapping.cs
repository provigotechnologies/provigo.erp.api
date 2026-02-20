using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProviGo.Common.Models
{
    [Table("TrainerShiftMappings")]
    public class TrainerShiftMapping
    {
        [Key]
        public int MappingId { get; set; }

        [Required]
        public Guid TrainerId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [ForeignKey(nameof(TrainerId))]
        public User User { get; set; }

        [ForeignKey(nameof(ShiftId))]
        public Shift Shift { get; set; }
    }
}
