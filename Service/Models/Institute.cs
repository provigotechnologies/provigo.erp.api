namespace InstituteService.Models
{
    public class Institute
    {
        public int Id { get; set; } // Auto-increment
        public string Name { get; set; }
        public string LogoPath { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
