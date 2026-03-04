namespace ShiftService.DTOs
{
    public class StudentEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
