using PaymentGatewayService.Data;
using PaymentGatewayService.DTOs;
using PaymentGatewayService.Models;
using Razorpay.Api;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public PaymentService(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }
  
        private string? GetKeyId() => _config["Razorpay:KeyId"];    
        private string? GetKeySecret() => _config["Razorpay:KeySecret"];

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var key = GetKeyId();
            var secret = GetKeySecret();

            var client = new RazorpayClient(key, secret);

            var data = new Dictionary<string, object>
            {
                ["amount"] = (int)(request.Amount * 100), // paise
                ["currency"] = request.Currency ?? "INR",
                ["receipt"] = request.Receipt ?? $"rcpt_{Guid.NewGuid()}",
                ["payment_capture"] = 1
            };

            var order = client.Order.Create(data);

            // Save order in DB
            var record = new OrderRecord
            {
                OrderId = order["id"].ToString(),
                Amount = Convert.ToInt64(order["amount"])/100,
                Currency = order["currency"].ToString(),
                Receipt = order["receipt"].ToString(),
                Status = "created",
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerContact = request.CustomerContact,
            };

            _db.Orders.Add(record);
            await _db.SaveChangesAsync();

            return new CreateOrderResponse
            {
                OrderId = record.OrderId,
                Amount = record.Amount,
                Currency = record.Currency,
                KeyId = key
            };
        }

        public async Task<bool> VerifyAndSavePaymentAsync(VerifyPaymentRequest dto)
        {
            var secret = GetKeySecret();
            var data = $"{dto.razorpay_order_id}|{dto.razorpay_payment_id}";

            // compute and verify signature
            var generated = Utils.SignatureHelper.CreateSignature(data, secret??"");

            if (!string.Equals(generated, dto.razorpay_signature, StringComparison.OrdinalIgnoreCase))
                return false;

            // update DB
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == dto.razorpay_order_id);
            if (order != null)
            {
                order.PaymentId = dto.razorpay_payment_id??"";
                order.Signature = dto.razorpay_signature;
                order.Status = "paid";
                order.PaidAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return true;
        }
    }
}
