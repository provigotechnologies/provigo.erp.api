namespace ShiftService.DTOs
{
    public class ProductOfferingUpdateDto
    {
        public int ShiftId { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; }
    }
}