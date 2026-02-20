namespace InstituteService.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsActive { get; set; } = true;
        public int InstituteId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
