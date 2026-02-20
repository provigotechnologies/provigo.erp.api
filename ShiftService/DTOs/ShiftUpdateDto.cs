namespace ShiftService.DTOs
{
    public class ShiftUpdateDto
    {
        public int ProductId { get; set; }
        public Guid TrainerId { get; set; }
        public string ShiftName { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
