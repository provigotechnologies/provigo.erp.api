namespace AttendanceService.DTOs
{
    public class AttendanceDto
    {
        public int AttendanceId { get; set; }
        public Guid TenantId { get; set; }
        public int CustomerId { get; set; }
        public int ShiftId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
    }

}