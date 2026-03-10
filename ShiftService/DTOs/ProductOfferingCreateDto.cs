namespace ShiftService.DTOs
{
    public class ProductOfferingCreateDto
    {
        public Guid BranchId { get; set; }
        public int ShiftId { get; set; }
        public int TrainerProductId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
