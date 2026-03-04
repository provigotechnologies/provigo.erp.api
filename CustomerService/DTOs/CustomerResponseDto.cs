namespace CustomerService.DTOs
{
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }

    }
}