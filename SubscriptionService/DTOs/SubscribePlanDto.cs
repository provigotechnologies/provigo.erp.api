namespace SubscriptionService.DTOs
{
    public class SubscribePlanDto
    {
        public Guid TenantDetailsId { get; set; }
        public int PlanId { get; set; }
        public string PaymentReference { get; set; } = "";
    }

    public class SubscriptionStatusDto
    {
        public string PlanName { get; set; } = "";
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int RemainingDays { get; set; }
    }
}
