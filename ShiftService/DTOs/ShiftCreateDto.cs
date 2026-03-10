namespace ShiftService.DTOs
{
    public class ShiftCreateDto
    {
        public Guid BranchId { get; set; }
        public string ShiftName { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
