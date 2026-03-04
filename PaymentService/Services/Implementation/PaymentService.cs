using Humanizer;
using IdentityService.Data;
using IdentityService.Services;
using Microsoft.CodeAnalysis.Operations;
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
        public async Task<ApiResponse<PaymentResponseDto>> CreateOfflinePaymentAsync(
           PaymentCreateDto dto,
           Guid branchId,
           Guid tenantId)
        {
            // 🔹 Add tenant header
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            // 🔹 Fetch order
            var response = await _httpClient.GetAsync(
             $"{_config["Services:OrderService"]}/api/orders/{dto.OrderId}?branchId={branchId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OrderService Error: {response.StatusCode}");

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrderResponseDto>>>();
            var order = apiResponse?.Data?.FirstOrDefault();

            if (order == null)
                throw new Exception("Order not found");

            var orderBalance = order.GrandTotal - order.PaidAmount;

            if (dto.PaidAmount > orderBalance)
                return ApiResponseFactory.Failure<PaymentResponseDto>("Paid amount exceeds order balance");

            // 🔹 Create a new payment entry for this offline payment
            var payment = new AppPayment
            {
                TenantId = tenantId,
                BranchId = branchId,
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
            $"{_config["Services:OrderService"]}/api/orders/{dto.OrderId}/update-payment?branchId={branchId}",
            new { PaidAmount = totalPaid });

            return ApiResponseFactory.Success(new PaymentResponseDto
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
     PaymentTransactionCreateDto request, Guid branchId, Guid tenantId)
        {
            var key = GetKeyId();
            var secret = GetKeySecret();
            var client = new RazorpayClient(key, secret);

            // 🔹 Add tenant header for multi-tenant support
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            // 🔹 Fetch order from OrderService
             var orderResponse = await _httpClient.GetAsync(
                $"{_config["Services:OrderService"]}/api/orders/{request.OrderId}?branchId={branchId}");

            if (!orderResponse.IsSuccessStatusCode)
            {
                var error = await orderResponse.Content.ReadAsStringAsync();
                throw new Exception($"OrderService Error: {orderResponse.StatusCode} - {error}");
            }

            var apiOrder = await orderResponse.Content
                .ReadFromJsonAsync<ApiResponse<List<OrderResponseDto>>>();
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
                BranchId = branchId,
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
        public async Task<bool> VerifyOnlinePaymentAsync(
     VerifyPaymentTransactionRequestDto dto, Guid branchId, Guid tenantId)
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
            $"{_config["Services:OrderService"]}/api/orders/{payment.OrderId}/update-payment?branchId={branchId}",
            new { PaidAmount = totalOrderPaid });

            return true;
        }


        //  GET PAYMENT
        public async Task<ApiResponse<List<AppPayment>>> GetPaymentsAsync(
           PaginationRequest request,
           bool includeInactive,
           Guid branchId, 
           Guid tenantId)
        {
            try
            {
                // Base query
                var query = _db.Payments
                    .Where(p => p.TenantId == tenantId && p.BranchId == branchId)
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
           int orderId,
           Guid branchId, 
           Guid tenantId)
        {
            // 1️⃣ Fetch all payments (offline + online) for this order
            var payments = await _db.Payments
                .Where(p => p.OrderId == orderId && p.TenantId == tenantId && p.BranchId == branchId)
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


        //  REFUND OFFLINE PAYMENT
        public async Task<ApiResponse<RefundResponseDto>> CreateOfflineRefundAsync(
        RefundCreateDto dto, Guid branchId, Guid tenantId)
        {
            try
            {
                if (dto.RefundAmount <= 0)
                    return ApiResponseFactory.Failure<RefundResponseDto>("Invalid refund amount");

                var payment = await _db.Payments
                    .Include(p => p.Refunds)
                    .FirstOrDefaultAsync(p =>
                        p.PaymentId == dto.PaymentId &&
                        p.TenantId == tenantId &&
                        p.Mode != "ONLINE");

                if (payment == null)
                    return ApiResponseFactory.Failure<RefundResponseDto>("Offline payment not found");

                var totalRefunded = payment.Refunds
                    .Where(r => r.Status == "Success")
                    .Sum(r => r.RefundAmount);

                var remainingRefundable = payment.PaidAmount - totalRefunded;

                if (dto.RefundAmount > remainingRefundable)
                    return ApiResponseFactory.Failure<RefundResponseDto>(
                        "Refund exceeds remaining paid amount");

                // 🔹 Create Refund Record
                var refund = new Refund
                {
                    TenantId = tenantId,
                    BranchId = branchId,
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    RefundAmount = dto.RefundAmount,
                    Reason = dto.Reason,
                    Status = "Success",
                    Mode = dto.Mode, // CASH / UPI
                    CreatedAt = DateTime.UtcNow
                };

                _db.Refunds.Add(refund);

                // 🔹 Update Payment Status
                var newTotalRefunded = totalRefunded + dto.RefundAmount;

                payment.Status = newTotalRefunded >= payment.PaidAmount
                    ? "Refunded"
                    : "Partially Refunded";

                await _db.SaveChangesAsync();

                // ======================================
                // 🔥 ORDER STATUS UPDATE (NO PaidAmount)
                // ======================================

                var totalPayments = await _db.Payments
                    .Where(p => p.OrderId == payment.OrderId &&
                                p.TenantId == tenantId && p.BranchId == branchId)
                    .SumAsync(p => p.PaidAmount);

                var totalOrderRefunds = await _db.Refunds
                    .Where(r => r.OrderId == payment.OrderId &&
                                r.TenantId == tenantId &&
                                r.Status == "Success")
                    .SumAsync(r => r.RefundAmount);

                var netPaid = totalPayments - totalOrderRefunds;

                string orderStatus;

                if (netPaid <= 0)
                    orderStatus = "Unpaid";
                else if (netPaid < totalPayments)
                    orderStatus = "Partially Paid";
                else
                    orderStatus = "Paid";

                _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
                _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

                await _httpClient.PutAsJsonAsync(
                    $"{_config["Services:OrderService"]}/api/orders/{payment.OrderId}/update-status?branchId={branchId}",
                    new { PaymentStatus = orderStatus });

                // ======================================

                return ApiResponseFactory.Success(new RefundResponseDto
                {
                    RefundId = refund.RefundId,
                    PaymentId = refund.PaymentId,
                    RefundAmount = refund.RefundAmount,
                    Reason = refund.Reason,
                    Status = refund.Status,
                    CreatedAt = refund.CreatedAt
                }, "Offline refund processed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<RefundResponseDto>("Refund failed: " + ex.Message);
            }
        }


        //  REFUND ONLINE PAYMENT
        public async Task<ApiResponse<RefundResponseDto>> CreateOnlineRefundAsync(
     RefundCreateDto dto, Guid branchId, Guid tenantId)
        {
            if (dto.RefundAmount <= 0)
                return ApiResponseFactory.Failure<RefundResponseDto>("Invalid refund amount");

            var payment = await _db.Payments
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p =>
                    p.PaymentId == dto.PaymentId &&
                    p.TenantId == tenantId && 
                    p.Mode == "ONLINE");

            if (payment == null)
                return ApiResponseFactory.Failure<RefundResponseDto>("Online payment not found");

            var transaction = payment.Transactions
                .Where(t => t.Status == "Success")
                .OrderByDescending(t => t.PaidAt)
                .FirstOrDefault();

            if (transaction == null)
                return ApiResponseFactory.Failure<RefundResponseDto>("Transaction not found");

            var client = new RazorpayClient(GetKeyId(), GetKeySecret());

            var options = new Dictionary<string, object>
            {
                { "amount", (int)(dto.RefundAmount * 100) }
            };

            var razorRefund = client.Payment
                .Fetch(transaction.GatewayPaymentId)
                .Refund(options);

            var refund = new Refund
            {
                TenantId = tenantId,
                BranchId = branchId,
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                RefundAmount = dto.RefundAmount,
                Reason = dto.Reason,
                Status = "Pending",
                Mode = "ONLINE",
                GatewayRefundId = razorRefund["id"]?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Refunds.Add(refund);
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success(new RefundResponseDto
            {
                RefundId = refund.RefundId,
                RefundAmount = refund.RefundAmount,
                Status = refund.Status
            }, "Refund initiated successfully");
        }


        // 🔹 VERIFY ONLINE REFUND
        public async Task<bool> VerifyOnlineRefundAsync(
      string gatewayRefundId, Guid branchId, Guid tenantId)
        {
            var refund = await _db.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r =>
                    r.GatewayRefundId == gatewayRefundId &&
                    r.TenantId == tenantId);

            if (refund == null || refund.Status == "Success")
                return false;

            var client = new RazorpayClient(GetKeyId(), GetKeySecret());

            var razorRefund = client.Refund.Fetch(gatewayRefundId);

            if (razorRefund["status"]?.ToString() != "processed")
                return false;

            // ✅ Update Refund Status Only
            refund.Status = "Success";

            var payment = refund.Payment;

            // ✅ Update Payment Status Only (No Amount Calculation)
            payment.Status = "Refunded";

            await _db.SaveChangesAsync();

            // ✅ Update Order Status Only (No Amount Update)
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

            await _httpClient.PutAsJsonAsync(
                $"{_config["Services:OrderService"]}/api/orders/{payment.OrderId}/update-status?branchId={branchId}",
                new { PaymentStatus = "Refunded" });

            return true;
        }


    }
}
