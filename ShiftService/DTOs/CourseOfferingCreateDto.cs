namespace ShiftService.DTOs
{
    public class CourseOfferingCreateDto
    {
        public int ShiftId { get; set; }
        public int TrainerCourseId { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
