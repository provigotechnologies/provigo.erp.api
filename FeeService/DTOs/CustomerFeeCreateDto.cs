namespace FeeService.DTOs
{
    public class CustomerFeeCreateDto
    {
        public int InstituteId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public decimal TotalAmount { get; set; }
    }


    public class CustomerFeeResponseDto
    {
        public int CustomerFeeId { get; set; }
        public int InstituteId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public bool IsActive { get; set; }
    }

}


