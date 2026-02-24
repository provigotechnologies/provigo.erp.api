namespace AttendanceService.DTOs
{
    public class AttendanceCreateDto
    {
        public int CustomerId { get; set; }
        public int ShiftId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
    }
}
