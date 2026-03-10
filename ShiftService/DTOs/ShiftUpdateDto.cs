namespace ShiftService.DTOs
{
    public class ShiftUpdateDto
    {
        public Guid BranchId { get; set; }
        public string ShiftName { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
