namespace MasterService.Models
{
    public class ExpenseMaster
    {
        public int ExpenseId { get; set; }
        public string ExpenseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
