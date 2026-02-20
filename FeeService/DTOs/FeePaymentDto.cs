namespace FeeService.DTOs
{
    public class FeePaymentDto
    {
        public int CustomerFeeId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "Cash";

    }

    public class FeePaymentResponseDto
    {
        public int FeePaymentId { get; set; }
        public int CustomerFeeId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "";
        public DateTime PaidOn { get; set; }
        public bool IsActive { get; set; }

    }

}
