namespace MasterService.Models
{
    public class DepartmentMaster
    {
        public int DeparatmentId { get; set; }
        public string DeparatmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
