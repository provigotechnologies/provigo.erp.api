namespace MasterService.Models
{
    public class HolidayMaster
    {
        public int HoidayId { get; set; }
        public string HoidayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
