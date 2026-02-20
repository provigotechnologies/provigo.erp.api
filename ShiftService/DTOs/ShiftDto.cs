namespace ShiftService.DTOs
{
    public class ShiftDto
    {
        public int ShiftId { get; set; }
        public Guid TenantId { get; set; }
        public int ProductId { get; set; }
        public Guid TrainerId { get; set; }
        public string ShiftName { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}
