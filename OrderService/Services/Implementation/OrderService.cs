using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Services.Interface;
using OrderService.Services.Internal;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Providers;
using ProviGo.Common.Response;
using ProviGo.Common.Services;

namespace OrderService.Services.Implementation
{
    public class OrderService(
        TenantDbContext db,
        IGenericRepository<Order> repo,
        TenantProvider tenantProvider) : IOrderService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Order> _repo = repo;
        private readonly TenantProvider _tenantProvider = tenantProvider;

        // 🔹 Helper: Map Order Entity -> OrderDto
        private OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                TenantId = order.TenantId,
                BranchId = order.BranchId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                SubTotal = order.SubTotal,
                DiscountTotal = order.DiscountTotal,
                TaxTotal = order.TaxTotal,
                GrandTotal = order.GrandTotal,
                PaidAmount = order.PaidAmount,
                BalanceAmount = order.BalanceAmount,
                CreatedAt = order.CreatedOn,

                Items = order.OrderItems?.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList() ?? new(),

                Discounts = order.OrderDiscounts?.Select(d => new OrderDiscountResponseDto
                {
                    DiscountId = d.DiscountId,
                    DiscountAmount = d.DiscountAmount
                }).ToList() ?? new(),

                Taxes = order.OrderTaxes?.Select(t => new OrderTaxResponseDto
                {
                    TaxId = t.TaxId,
                    TaxAmount = t.TaxAmount
                }).ToList() ?? new(),

                Charges = order.OrderCharges?.Select(c => new OrderChargeResponseDto
                {
                    ChargeId = c.ChargeId,
                    ChargeAmount = c.ChargeAmount
                }).ToList() ?? new()
            };
        }

        // ✅ CREATE ORDER
        public async Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(OrderCreateDto dto)
        {
            try
            {
                var tenantId = _tenantProvider.TenantId;

                // 🔒 Validate Branch
                if (!await _db.Branches
                        .AnyAsync(b => b.BranchId == dto.BranchId && b.TenantId == tenantId))
                    return ApiResponseFactory.Failure<OrderResponseDto>("Invalid branch");

                // 🔒 Validate Customer
                if (!await _db.Customers
                        .AnyAsync(c => c.CustomerId == dto.CustomerId && c.TenantId == tenantId))
                    return ApiResponseFactory.Failure<OrderResponseDto>("Invalid customer");

                // 🔒 Validate Products
                var productIds = dto.Items.Select(i => i.ProductId).ToList();
                var products = await _db.Products
                    .Where(p => productIds.Contains(p.ProductId)
                             && p.TenantId == tenantId
                             && p.IsActive)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    return ApiResponseFactory.Failure<OrderResponseDto>("Invalid product detected");

                // 🔹 Prepare Raw Items
                var rawItems = dto.Items.Select(i =>
                {
                    var product = products.First(p => p.ProductId == i.ProductId);
                    return (
                        ProductId: product.ProductId,
                        Quantity: i.Quantity,
                        UnitPrice: product.TotalFee
                    );
                }).ToList();

                // 🔹 Fetch Discounts, Taxes, Charges from master tables
                var discountIds = dto.Discounts?.Select(d => d.DiscountId).ToList() ?? new();
                var taxIds = dto.Taxes?.Select(t => t.TaxId).ToList() ?? new();
                var chargeIds = dto.Charges?.Select(c => c.ChargeId).ToList() ?? new();

                var discountsFromDb = await _db.Discounts
                    .Where(d => discountIds.Contains(d.DiscountId)
                             && d.TenantId == tenantId
                             && d.IsActive)
                    .ToListAsync();

                var taxesFromDb = await _db.Taxes
                    .Where(t => taxIds.Contains(t.TaxId)
                             && t.TenantId == tenantId
                             && t.IsActive)
                    .ToListAsync();

                var chargesFromDb = await _db.Charges
                    .Where(c => chargeIds.Contains(c.ChargeId)
                             && c.TenantId == tenantId
                             && c.IsActive)
                    .ToListAsync();

                // ✅ CALCULATE totals
                OrderCalculation.BuildAndCalculate(
                    rawItems,
                    discountsFromDb,
                    taxesFromDb,
                    chargesFromDb,
                    out var orderItems,
                    out var orderDiscounts,
                    out var orderTaxes,
                    out var orderCharges,
                    out var subTotal,
                    out var discountTotal,
                    out var taxTotal,
                    out var chargeTotal,
                    out var grandTotal);

                // PAYMENT LOGIC 
                var paidAmount = dto.PaidAmount < 0 ? 0 : dto.PaidAmount;

                if (paidAmount > grandTotal)
                    return ApiResponseFactory.Failure<OrderResponseDto>(
                        "Paid amount cannot exceed grand total");

                var balanceAmount = grandTotal - paidAmount;

                // 🔹 Create Order entity
                var order = new Order
                {
                    TenantId = tenantId,
                    BranchId = dto.BranchId,
                    CustomerId = dto.CustomerId,
                    OrderDate = dto.OrderDate == default
                        ? DateTime.UtcNow
                        : dto.OrderDate,

                    Status = balanceAmount == 0 ? "Completed"
                        : paidAmount > 0 ? "Confirmed"
                        : "Created",

                    SubTotal = subTotal,
                    DiscountTotal = discountTotal,
                    TaxTotal = taxTotal,
                    GrandTotal = grandTotal,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,

                    OrderItems = orderItems,
                    OrderDiscounts = orderDiscounts,
                    OrderTaxes = orderTaxes,
                    OrderCharges = orderCharges
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                return ApiResponseFactory.Success(MapToDto(order), "Order created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<OrderResponseDto>(
                    "Order creation failed",
                    new List<string> { ex.Message });
            }
        }

        // ✅ LIST ORDERS
        public async Task<ApiResponse<List<OrderResponseDto>>> GetOrdersAsync(
        PaginationRequest request,
        bool includeInactive,
        Guid branchId)
        {
            // 1️⃣ Build query
            var query = _db.Orders
                .Where(o => o.TenantId == _tenantProvider.TenantId && o.BranchId == branchId)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderDiscounts)
                .Include(o => o.OrderTaxes)
                .Include(o => o.OrderCharges)
                .AsNoTracking();

            // 2️⃣ Fetch paged result from repository
            var pagedResult = await _repo.GetPagedAsync(
                query,
                request,
                o => includeInactive || o.Status != "Cancelled");

            // 3️⃣ Map Orders -> DTOs
            var dtoList = pagedResult.Items.Select(MapToDto).ToList();

            // 4️⃣ Wrap DTOs in PagedResult
            var pagedDtoResult = new PagedResult<OrderResponseDto>
            {
                Items = dtoList,
                Pagination = pagedResult.Pagination  // reuse repo pagination
            };

            // 5️⃣ Return ApiResponse
            return ApiResponseFactory.PagedSuccess(pagedDtoResult, "Orders fetched successfully");
        }


        public async Task<ApiResponse<List<OrderResponseDto>>> GetOrderByIdAsync(int orderId, Guid branchId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderTaxes)
                .Include(o => o.OrderDiscounts)
                .Include(o => o.OrderCharges)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == _tenantProvider.TenantId && o.BranchId == branchId);

            if (order == null)
                return ApiResponseFactory.Failure<List<OrderResponseDto>>("Order not found");

            return ApiResponseFactory.Success(new List<OrderResponseDto> { MapToDto(order) });
        }


        // ✅ UPDATE STATUS ONLY
        public async Task<ApiResponse<string>> UpdateOrderAsync(int orderId, OrderUpdateDto dto, Guid branchId)
        {
            var allowedStatuses = new[] { "Created", "Confirmed", "Completed", "Cancelled" };

            if (!allowedStatuses.Contains(dto.Status))
                return ApiResponseFactory.Failure<string>("Invalid status");

            var affectedRows = await _db.Orders
                .Where(o => o.OrderId == orderId && o.TenantId == _tenantProvider.TenantId && o.BranchId == branchId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, dto.Status));

            if (affectedRows == 0)
                return ApiResponseFactory.Failure<string>("Order not found");

            return ApiResponseFactory.Success("Order updated successfully");
        }


        // ✅ DELETE ORDER
        public async Task<ApiResponse<string>> RemoveOrderAsync(int orderId, Guid branchId)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == _tenantProvider.TenantId && o.BranchId == branchId);

            if (order == null)
                return ApiResponseFactory.Failure<string>("Order not found");

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Order deleted successfully");
        }


        // ✅ UPDATE ORDER PAYMENT
        public async Task<ApiResponse<string>> UpdatePaymentAsync(
         int orderId,
         decimal paidAmount,
         Guid branchId)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId &&
                    o.TenantId == _tenantProvider.TenantId &&
                    o.BranchId == branchId);

            if (order == null)
                return ApiResponseFactory.Failure<string>("Order not found");

            // ✅ SET — NOT ADD
            order.PaidAmount = paidAmount;

            order.BalanceAmount = order.GrandTotal - order.PaidAmount;

            if (order.BalanceAmount <= 0)
            {
                order.BalanceAmount = 0;
                order.Status = "Completed";
            }
            else
            {
                order.Status = "Confirmed";
            }

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Payment updated successfully");
        }


        // ✅ UPDATE REFUND PAYMENT
        public async Task<ApiResponse<string>> UpdateRefundAsync(
        int orderId,
        decimal refundAmount,
        Guid branchId)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId &&
                    o.TenantId == _tenantProvider.TenantId &&
                    o.BranchId == branchId);

            if (order == null)
                return ApiResponseFactory.Failure<string>("Order not found");

            order.PaidAmount -= refundAmount;

            if (order.PaidAmount < 0)
                order.PaidAmount = 0;

            order.BalanceAmount = order.GrandTotal - order.PaidAmount;

            if (order.PaidAmount == 0)
                order.Status = "Created";
            else
                order.Status = "Confirmed";

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Refund updated successfully");
        }


    }
}