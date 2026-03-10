namespace ShiftService.DTOs
{
    public class ShiftResponseDto
    {
        public int ShiftId { get; set; }
        public Guid BranchId { get; set; }
        public string ShiftName { get; set; } = "";
        public bool IsActive { get; set; }
    }

}
