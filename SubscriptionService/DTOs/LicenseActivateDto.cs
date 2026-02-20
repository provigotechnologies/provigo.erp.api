namespace SubscriptionService.DTOs
{
    public class LicenseActivateDto
    {
        public Guid TenantDetailsId { get; set; }
        public string PaymentReference { get; set; } = "";
    }

    public class LicenseStatusDto
    {
        public bool IsValid { get; set; }
        public DateTime SupportExpiry { get; set; }
    }
}