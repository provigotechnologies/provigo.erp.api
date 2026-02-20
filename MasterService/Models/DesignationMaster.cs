namespace MasterService.Models
{
    public class DesignationMaster
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
