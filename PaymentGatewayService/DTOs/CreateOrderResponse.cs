using System.Text.Json.Serialization;

namespace PaymentGatewayService.DTOs
{
    public class CreateOrderResponse
    {
        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }
        [JsonPropertyName("amount")]
        public long? Amount { get; set; }  // paise
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; } // public key to pass to frontend
    }
}
