namespace MasterService.Models
{
    public class TableMaster
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
