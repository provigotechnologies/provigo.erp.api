using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using PaymentService.DTOs;
using PaymentService.Services.Interface;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;
using System.Net.Http;
using AppPayment = ProviGo.Common.Models.Payment;
using AppPaymentTransaction = ProviGo.Common.Models.PaymentTransaction;
using DbRefund = ProviGo.Common.Models.Refund;
using RazorpayClient = Razorpay.Api.RazorpayClient;
using RazorpayPayment = Razorpay.Api.Payment;

namespace PaymentService.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly TenantDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly IGenericRepository<AppPayment> _repo;

        public PaymentService(
            IConfiguration config,
            TenantDbContext db,
            HttpClient httpClient,
            IGenericRepository<AppPayment> repo)
        {
            _config = config;
            _db = db;
            _httpClient = httpClient;
            _repo = repo;
        }

        private string? GetKeyId() => _config["Razorpay:KeyId"];
        private string? GetKeySecret() => _config["Razorpay:KeySecret"];



        //  OFFLINE PAYMENT
        public async Task<ApiResponse<PaymentDto>> CreateOfflinePaymentAsync(
           PaymentCreateDto dto,
           Guid tenantId)
        {
            // 🔹 Add tenant header
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            // 🔹 Fetch order
            var response = await _httpClient.GetAsync(
                $"{_config["Services:OrderService"]}/api/orders/{dto.OrderId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OrderService Error: {response.StatusCode}");

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrderDto>>>();
            var order = apiResponse?.Data?.FirstOrDefault();

            if (order == null)
                throw new Exception("Order not found");

            var orderBalance = order.GrandTotal - order.PaidAmount;

            if (dto.PaidAmount > orderBalance)
                return ApiResponseFactory.Failure<PaymentDto>("Paid amount exceeds order balance");

            // 🔹 Create a new payment entry for this offline payment
            var payment = new AppPayment
            {
                TenantId = tenantId,
                BranchId = dto.BranchId,
                OrderId = dto.OrderId,
                TotalAmount = orderBalance,
                PaidAmount = dto.PaidAmount,       // only this installment
                Status = dto.PaidAmount == orderBalance ? "Paid" : "Partial",
                Mode = dto.Mode,
                CreatedAt = DateTime.UtcNow        // keeps payment date
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // 🔹 Update Order PaidAmount & BalanceAmount
            decimal totalPaid = await _db.Payments
            .Where(p => p.OrderId == dto.OrderId)
            .SumAsync(p => p.PaidAmount);

            await _httpClient.PutAsJsonAsync(
                $"{_config["Services:OrderService"]}/api/orders/{dto.OrderId}/update-payment",
                new { PaidAmount = totalPaid });

            return ApiResponseFactory.Success(new PaymentDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                TotalAmount = payment.TotalAmount,
                PaidAmount = payment.PaidAmount,
                BalanceAmount = payment.TotalAmount - order.PaidAmount - dto.PaidAmount,
                PaymentStatus = payment.Status,
                Mode = dto.Mode,
                CreatedAt = payment.CreatedAt
            });
        }

        //  CREATE RAZORPAY ORDER  
        public async Task<PaymentTransactionResponseCreateDto> CreateOnlinePaymentAsync(
     PaymentTransactionRequestCreateDto request, Guid tenantId)
        {
            var key = GetKeyId();
            var secret = GetKeySecret();
            var client = new RazorpayClient(key, secret);

            // 🔹 Add tenant header for multi-tenant support
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            // 🔹 Fetch order from OrderService
            var orderResponse = await _httpClient.GetAsync(
                $"{_config["Services:OrderService"]}/api/orders/{request.OrderId}");

            if (!orderResponse.IsSuccessStatusCode)
            {
                var error = await orderResponse.Content.ReadAsStringAsync();
                throw new Exception($"OrderService Error: {orderResponse.StatusCode} - {error}");
            }

            var apiOrder = await orderResponse.Content
                .ReadFromJsonAsync<ApiResponse<List<OrderDto>>>();
            var order = apiOrder?.Data?.FirstOrDefault();
            if (order == null)
                throw new Exception("Order not found");

            // 🔹 Calculate remaining balance for the order
            decimal remainingBalance = order.GrandTotal - order.PaidAmount;
            if (remainingBalance <= 0)
                throw new Exception("Order is already fully paid");

            // 🔹 Create a new Payment row for this online installment
            var onlinePayment = new AppPayment
            {
                TenantId = tenantId,
                BranchId = request.BranchId,
                OrderId = request.OrderId,
                TotalAmount = remainingBalance, // remaining balance due
                PaidAmount = 0,                 // will update after verification
                Status = "Pending",
                Mode = "ONLINE",
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(onlinePayment);
            await _db.SaveChangesAsync();

            // 🔹 Create Razorpay order for the actual payment amount
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(request.Amount * 100) }, // amount in paise
                { "currency", request.Currency ?? "INR" },
                { "receipt", request.Receipt ?? Guid.NewGuid().ToString() },
                { "payment_capture", 1 }
            };

            var razorOrder = client.Order.Create(options);

            // 🔹 Create PaymentTransaction row
            var transaction = new AppPaymentTransaction
            {
                PaymentId = onlinePayment.PaymentId,
                OrderId = onlinePayment.OrderId,       // ✅ assign OrderId
                GatewayOrderId = razorOrder["id"].ToString(),
                Amount = request.Amount,               // actual installment
                Currency = request.Currency,
                Status = "Created",
                Mode = "Razorpay",
                CreatedAt = DateTime.UtcNow
            };

            _db.PaymentTransactions.Add(transaction);
            await _db.SaveChangesAsync();

            return new PaymentTransactionResponseCreateDto
            {
                OrderId = transaction.GatewayOrderId,
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency,
                KeyId = key
            };
        }


        // 🔹 VERIFY ONLINE PAYMENT
        public async Task<bool> VerifyAndSavePaymentTransactionAsync(
     VerifyPaymentTransactionRequestDto dto, Guid tenantId)
        {

            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            var key = GetKeyId();
            var secret = GetKeySecret();

            // 🔹 Verify signature
            var data = $"{dto.razorpay_order_id}|{dto.razorpay_payment_id}";
            var generated = Utils.SignatureHelper.CreateSignature(data, secret ?? "");

            if (!string.Equals(generated, dto.razorpay_signature, StringComparison.OrdinalIgnoreCase))
                return false;

            // 🔹 Fetch transaction with parent Payment
            var transaction = await _db.PaymentTransactions
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.GatewayOrderId == dto.razorpay_order_id);

            if (transaction == null) return false;
            if (transaction.Status == "Success") return true;

            // 🔹 Update transaction
            transaction.GatewayPaymentId = dto.razorpay_payment_id;
            transaction.Signature = dto.razorpay_signature;
            transaction.Status = "Success";
            transaction.PaidAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(); // 🔥 SAVE TRANSACTION FIRST

            // 🔹 Now update parent ONLINE Payment
            var payment = transaction.Payment;
            if (payment == null)
                throw new Exception("Parent payment not found");

            var totalPaid = await _db.PaymentTransactions
                .Where(t => t.PaymentId == payment.PaymentId && t.Status == "Success")
                .SumAsync(t => t.Amount);

            payment.PaidAmount = totalPaid;
            payment.Status = payment.PaidAmount >= payment.TotalAmount ? "Paid" : "Partial";

            await _db.SaveChangesAsync(); // 🔥 SAVE PAYMENT

            // 🔹 Update OrderService PaidAmount: sum of all payments (offline + online)
            var totalOrderPaid = await _db.Payments
                .Where(p => p.OrderId == payment.OrderId)
                .SumAsync(p => p.PaidAmount);

            await _httpClient.PutAsJsonAsync(
                $"{_config["Services:OrderService"]}/api/orders/{payment.OrderId}/update-payment",
                new { PaidAmount = totalOrderPaid });

            return true;
        }


        //  GET PAYMENT
        public async Task<ApiResponse<List<AppPayment>>> GetPaymentsAsync(
           PaginationRequest request,
           bool includeInactive, Guid tenantId)
        {
            try
            {
                // Base query
                var query = _db.Payments
                    .Where(p => p.TenantId == tenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(
                query,
                request,
                _ => true
            );

                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Payments fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<AppPayment>>(ex.Message, ["Database error occurred"]);

            }
        }


        // 🔹 GET FULL PAYMENT HISTORY FOR AN ORDER
        public async Task<ApiResponse<List<OrderPaymentHistoryDto>>> GetOrderPaymentHistoryAsync(
           int orderId, Guid tenantId)
        {
            // 1️⃣ Fetch all payments (offline + online) for this order
            var payments = await _db.Payments
                .Where(p => p.OrderId == orderId && p.TenantId == tenantId)
                .Include(p => p.Transactions) // include online transactions
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            if (!payments.Any())
                return ApiResponseFactory.Failure<List<OrderPaymentHistoryDto>>("No payments found");

            var paymentHistory = new List<OrderPaymentHistoryDto>();

            foreach (var payment in payments)
            {
                // 🔹 Add offline payment entry
                if (payment.Mode != "ONLINE")
                {
                    paymentHistory.Add(new OrderPaymentHistoryDto
                    {
                        PaymentId = payment.PaymentId,
                        Amount = payment.PaidAmount,
                        Mode = payment.Mode,
                        Status = payment.Status,
                        PaidAt = payment.CreatedAt
                    });
                }

                // 🔹 Add online payment transactions
                if (payment.Mode == "ONLINE" && payment.Transactions != null)
                {
                    foreach (var tx in payment.Transactions
                        .Where(t => t.Status == "Success")
                        .OrderBy(t => t.PaidAt))
                    {
                        paymentHistory.Add(new OrderPaymentHistoryDto
                        {
                            PaymentId = payment.PaymentId,
                            TransactionId = tx.TransactionId,
                            Amount = tx.Amount,
                            Mode = tx.Mode, 
                            Status = tx.Status,
                            PaidAt = tx.PaidAt ?? payment.CreatedAt
                        });
                    }
                }
            }

            return ApiResponseFactory.Success(paymentHistory, "Order payment history fetched successfully");
        }


        // 4 REFUND PAYMENT
        public async Task<ApiResponse<RefundDto>> CreateRefundAsync(
         RefundCreateDto dto, Guid tenantId)
        {
            try
            {
                if (dto.RefundAmount <= 0)
                    return ApiResponseFactory.Failure<RefundDto>("Invalid refund amount");

                var payment = await _db.Payments
                    .Include(p => p.Transactions)
                    .Include(p => p.Refunds)
                    .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId &&
                                              p.TenantId == tenantId);

                if (payment == null)
                    return ApiResponseFactory.Failure<RefundDto>("Payment not found");

                var totalRefunded = payment.Refunds
                    .Where(r => r.Status == "Success")
                    .Sum(r => r.RefundAmount);

                var refundableAmount = payment.PaidAmount;

                if (dto.RefundAmount > refundableAmount)
                    return ApiResponseFactory.Failure<RefundDto>(
                        "Refund exceeds remaining paid amount");

                string? gatewayRefundId = null;

                // 🔥 ONLINE REFUND
                if (payment.Mode == "ONLINE")
                {
                    var transaction = payment.Transactions
                        .Where(t => t.Status == "Success")
                        .OrderByDescending(t => t.PaidAt)
                        .FirstOrDefault();

                    if (transaction == null)
                        return ApiResponseFactory.Failure<RefundDto>(
                            "No successful online transaction found");

                    if (string.IsNullOrEmpty(transaction.GatewayPaymentId))
                        return ApiResponseFactory.Failure<RefundDto>(
                            "Invalid gateway payment id");

                    var client = new RazorpayClient(GetKeyId(), GetKeySecret());

                    var options = new Dictionary<string, object>
                    {
                        { "amount", (int)(dto.RefundAmount * 100) },
                        { "speed", "normal" }
                    };

                    var razorpayRefund = client.Payment
                        .Fetch(transaction.GatewayPaymentId)
                        .Refund(options);

                    gatewayRefundId = razorpayRefund["id"]?.ToString();
                }

                // 🔥 OFFLINE REFUND → No gateway call
                else
                {
                    gatewayRefundId = null;
                }

                // 🔥 Create Refund Record
                var refundEntity = new Refund
                {
                    TenantId = payment.TenantId,
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    RefundAmount = dto.RefundAmount,
                    Reason = dto.Reason,
                    Status = "Success",
                    Mode = payment.Mode == "ONLINE" ? "ONLINE" : dto.Mode,
                    GatewayRefundId = gatewayRefundId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Refunds.Add(refundEntity);

                // 🔥 Reduce PaidAmount
                payment.PaidAmount -= dto.RefundAmount;

                if (payment.PaidAmount == 0)
                    payment.Status = "Refunded";
                else
                    payment.Status = "Partial";

                await _db.SaveChangesAsync();

                // 🔥 Update Order Service
                decimal totalPaid = await _db.Payments
                    .Where(p => p.OrderId == payment.OrderId)
                    .SumAsync(p => p.PaidAmount);

                await _httpClient.PutAsJsonAsync(
                    $"{_config["Services:OrderService"]}/api/orders/{payment.OrderId}/update-payment",
                    new { PaidAmount = totalPaid });

                return ApiResponseFactory.Success(new RefundDto
                {
                    RefundId = refundEntity.RefundId,
                    PaymentId = refundEntity.PaymentId,
                    RefundAmount = refundEntity.RefundAmount,
                    Reason = refundEntity.Reason,
                    Status = refundEntity.Status,
                    CreatedAt = refundEntity.CreatedAt
                }, "Refund processed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<RefundDto>(
                    "Refund failed: " + ex.Message);
            }
        }

    }
}
