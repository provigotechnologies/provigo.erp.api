using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using PaymentService.DTOs;
using PaymentService.Services.Interface;
using Provigo.Common.Exceptions;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace PaymentService.Services.Implementation
{
    public class PaymentService(
 TenantDbContext db,
 IGenericRepository<Payment> repo,
 IIdentityProvider identityProvider) : IPaymentService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Payment> _repo = repo;
        private readonly IIdentityProvider _identityProvider = identityProvider;
        private Guid TenantId => _identityProvider.TenantId;


        public async Task<ApiResponse<PaymentDto>> CreatePaymentAsync(PaymentCreateDto dto)
        {
            try
            {
                var order = await _db.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId
                                           && o.TenantId == TenantId);

                if (order == null)
                    return ApiResponseFactory.Failure<PaymentDto>("Order not found");

                decimal totalPayable = dto.TotalPayable > 0
                    ? dto.TotalPayable
                    : order.GrandTotal;

                var payment = new Payment
                {
                    TenantId = TenantId,
                    BranchId = dto.BranchId,
                    OrderId = dto.OrderId,
                    TotalPayable = totalPayable,
                    PaymentStatus = "Pending", // Always Pending
                    CreatedAt = DateTime.UtcNow
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                var responseDto = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    TenantId = payment.TenantId,
                    BranchId = payment.BranchId,
                    OrderId = payment.OrderId,
                    TotalPayable = payment.TotalPayable,
                    PaymentStatus = payment.PaymentStatus,
                    CreatedAt = payment.CreatedAt,
                    Transactions = new List<PaymentTransactionDto>(),
                    Refunds = new List<RefundDto>()
                };

                return ApiResponseFactory.Success(responseDto, "Payment created successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<PaymentDto>("Database error occurred");
            }
        }


        public async Task<ApiResponse<PaymentTransactionDto>> AddTransactionAsync(
            int paymentId,
            PaymentTransactionCreateDto dto)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId
                                           && p.TenantId == TenantId);

                if (payment == null)
                    return ApiResponseFactory.Failure<PaymentTransactionDto>("Payment not found");

                // 🔥 Prevent overpayment
                var totalPaidBefore = await _db.PaymentTransactions
                    .Where(t => t.PaymentId == paymentId && t.Status == "Success")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                if (totalPaidBefore + dto.Amount > payment.TotalPayable)
                    return ApiResponseFactory.Failure<PaymentTransactionDto>("Amount exceeds balance");

                // Add transaction
                var transaction = new PaymentTransaction
                {
                    PaymentId = paymentId,
                    Mode = dto.Mode,
                    Amount = dto.Amount,
                    GatewayRef = dto.GatewayRef,
                    Status = "Success"
                };

                _db.PaymentTransactions.Add(transaction);
                await _db.SaveChangesAsync();

                var totalPaid = totalPaidBefore + dto.Amount;

                // 🔥 Update status
                if (totalPaid >= payment.TotalPayable)
                    payment.PaymentStatus = "Completed";
                else if (totalPaid > 0)
                    payment.PaymentStatus = "Partial";
                else
                    payment.PaymentStatus = "Pending";

                await _db.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                var response = new PaymentTransactionDto
                {
                    TransactionId = transaction.TransactionId,
                    PaymentId = transaction.PaymentId,
                    Mode = transaction.Mode,
                    Amount = transaction.Amount,
                    GatewayRef = transaction.GatewayRef,
                    Status = transaction.Status
                };

                return ApiResponseFactory.Success(response, "Transaction added successfully");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return ApiResponseFactory.Failure<PaymentTransactionDto>("Transaction failed");
            }
        }


        public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.Transactions)
                    .Include(p => p.Refunds)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId
                                           && p.TenantId == TenantId);

                if (payment == null)
                    return ApiResponseFactory.Failure<PaymentDto>("Payment not found");

                var paymentDto = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    TenantId = payment.TenantId,
                    BranchId = payment.BranchId,
                    OrderId = payment.OrderId,
                    TotalPayable = payment.TotalPayable,
                    PaymentStatus = payment.PaymentStatus,
                    CreatedAt = payment.CreatedAt,
                    Transactions = payment.Transactions?
                        .Select(t => new PaymentTransactionDto
                        {
                            TransactionId = t.TransactionId,
                            PaymentId = t.PaymentId,
                            Mode = t.Mode,
                            Amount = t.Amount,
                            GatewayRef = t.GatewayRef,
                            Status = t.Status
                        }).ToList() ?? new List<PaymentTransactionDto>(),

                    Refunds = payment.Refunds?
                        .Select(r => new RefundDto
                        {
                            RefundId = r.RefundId,
                            RefundAmount = r.RefundAmount,
                            Reason = r.Reason,
                            CreatedAt = r.CreatedAt
                        }).ToList() ?? new List<RefundDto>()
                };

                return ApiResponseFactory.Success(paymentDto, "Payment fetched successfully");
            }
            catch (Exception)
            {
                return ApiResponseFactory.Failure<PaymentDto>("Database error occurred");
            }
        }


        public async Task<ApiResponse<List<Payment>>> GetPaymentsAsync(
            PaginationRequest request,
            bool includeInactive)
        {
            try
            {
                // Base query
                var query = _db.Payments
                    .Where(p => p.TenantId == TenantId)
                    .AsNoTracking();

                // Apply filter
                var pagedResult = await _repo.GetPagedAsync(query, request, i => includeInactive || i.PaymentStatus != "Deleted");


                // Wrap with standard response
                return ApiResponseFactory.PagedSuccess(
                    pagedResult,
                    "Payments fetched successfully"
                );
            }
            catch (Exception ex)
            {

                return ApiResponseFactory.Failure<List<Payment>>(ex.Message, ["Database error occurred"]);

            }
        }

        public async Task<ApiResponse<string>> UpdatePaymentAsync(int paymentId, PaymentUpdateDto dto)
        {
            try
            {
                int affectedRows = await _db.Payments
                   .Where(p => p.PaymentId == paymentId
                            && p.TenantId == TenantId)
                   .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.TotalPayable, dto.TotalPayable)
                   );

                if (affectedRows == 0)
                    throw new NotFoundException("Payment not found");

                return ApiResponseFactory.Success("Payment updated successfully");
            }
            catch (DbUpdateException)
            {
                return ApiResponseFactory.Failure<string>("Database error occurred");
            }
        }


        public async Task<ApiResponse<string>> RemovePaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _db.Payments
      .Include(p => p.Transactions)
      .Include(p => p.Refunds)
      .FirstOrDefaultAsync(p => p.PaymentId == paymentId
                             && p.TenantId == TenantId);

                if (payment == null)
                {
                    return ApiResponseFactory.Failure<string>("Payment not found");
                }

                _db.PaymentTransactions.RemoveRange(payment.Transactions);
                _db.Refunds.RemoveRange(payment.Refunds);
                _db.Payments.Remove(payment);

                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return ApiResponseFactory.Failure<string>("Delete failed");
                }

                return ApiResponseFactory.Success(
                    $"Payment {paymentId} deleted successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<string>(
                    "Database error occurred",
                    new List<string> { ex.Message }
                );
            }
        }


        public async Task<ApiResponse<RefundDto>> CreateRefundAsync(RefundCreateDto dto)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId
                                           && p.TenantId == TenantId);

                if (payment == null)
                    return ApiResponseFactory.Failure<RefundDto>("Payment not found");

                // 🔥 Total Paid
                var totalPaid = await _db.PaymentTransactions
                    .Where(t => t.PaymentId == dto.PaymentId && t.Status == "Success")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                // 🔥 Total Refunded
                var totalRefunded = await _db.Refunds
                    .Where(r => r.PaymentId == dto.PaymentId && r.Status == "Completed")
                    .SumAsync(r => (decimal?)r.RefundAmount) ?? 0;

                // 🔥 Prevent over refund
                if (totalRefunded + dto.RefundAmount > totalPaid)
                    return ApiResponseFactory.Failure<RefundDto>("Refund exceeds paid amount");

                var refund = new Refund
                {
                    TenantId = TenantId,
                    PaymentId = dto.PaymentId,
                    RefundAmount = dto.RefundAmount,
                    Reason = dto.Reason,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Refunds.Add(refund);
                await _db.SaveChangesAsync();

                // 🔥 Recalculate Net Paid
                var netPaid = totalPaid - (totalRefunded + dto.RefundAmount);

                if (netPaid >= payment.TotalPayable)
                    payment.PaymentStatus = "Completed";
                else if (netPaid > 0)
                    payment.PaymentStatus = "Partial";
                else
                    payment.PaymentStatus = "Pending";

                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                var response = new RefundDto
                {
                    RefundId = refund.RefundId,
                    PaymentId = refund.PaymentId,
                    RefundAmount = refund.RefundAmount,
                    Reason = refund.Reason,
                    Status = refund.Status,
                    CreatedAt = refund.CreatedAt
                };

                return ApiResponseFactory.Success(response, "Refund processed successfully");
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                return ApiResponseFactory.Failure<RefundDto>("Refund failed");
            }
        }


    }

}
