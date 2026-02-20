namespace InstituteService.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public decimal Fee { get; set; }
        public int InstituteId { get; set; }
    }
}
