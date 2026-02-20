namespace InstituteService.Models
{
    public class FeePayment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidOn { get; set; }
        public string PaymentMode { get; set; }
        public string Remarks { get; set; }
    }


}
