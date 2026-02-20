namespace InstituteService.Models
{
    public class Batch
    {
        public int Id { get; set; }
        public string BatchName { get; set; }
        public int CourseId { get; set; }
        public string TrainerName { get; set; }
    }
}
