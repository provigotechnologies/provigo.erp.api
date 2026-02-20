namespace MasterService.Models
{
    public class LocationMaster
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
