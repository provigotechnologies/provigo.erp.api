namespace AttendanceService.DTOs
{
    public class AttendanceDto
    {
        public Guid TenantDetailsId { get; set; }
        public int ShiftId { get; set; }
        public int CustomerId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
    }


    // Response
    public class AttendanceResponseDto
    {
        public int AttendanceId { get; set; }
        public int CustomerId { get; set; }
        public int ShiftId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }

}