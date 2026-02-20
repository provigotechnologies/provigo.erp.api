using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.DTOs;
using OrderService.Services.Interface;
using OrderService.Services.Internal;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace OrderService.Services.Implementation
{
    public class OrderService(
        TenantDbContext db,
        IGenericRepository<Order> repo,
        IIdentityProvider identityProvider) : IOrderService
    {
        private readonly TenantDbContext _db = db;
        private readonly IGenericRepository<Order> _repo = repo;
        private readonly IIdentityProvider _identityProvider = identityProvider;
        private Guid TenantId => _identityProvider.TenantId;

        // 🔹 Helper: Map Order Entity -> OrderDto
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
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
                CreatedAt = order.CreatedAt,

                Items = order.OrderItems?.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList() ?? new(),

                Discounts = order.OrderDiscounts?.Select(d => new OrderDiscountDto
                {
                    DiscountId = d.DiscountId,
                    DiscountAmount = d.DiscountAmount
                }).ToList() ?? new(),

                Taxes = order.OrderTaxes?.Select(t => new OrderTaxDto
                {
                    TaxId = t.TaxId,
                    TaxAmount = t.TaxAmount
                }).ToList() ?? new(),

                Charges = order.OrderCharges?.Select(c => new OrderChargeDto
                {
                    ChargeId = c.ChargeId,
                    ChargeAmount = c.ChargeAmount
                }).ToList() ?? new()
            };
        }

        // ✅ CREATE ORDER
        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(OrderCreateDto dto)
        {
            try
            {
                // 🔒 Validate Branch
                if (!await _db.Branches
                        .AnyAsync(b => b.BranchId == dto.BranchId && b.TenantId == TenantId))
                    return ApiResponseFactory.Failure<OrderDto>("Invalid branch");

                // 🔒 Validate Customer
                if (!await _db.Customers
                        .AnyAsync(c => c.CustomerId == dto.CustomerId && c.TenantId == TenantId))
                    return ApiResponseFactory.Failure<OrderDto>("Invalid customer");

                // 🔒 Validate Products
                var productIds = dto.Items.Select(i => i.ProductId).ToList();
                var products = await _db.Products
                    .Where(p => productIds.Contains(p.ProductId)
                             && p.TenantId == TenantId
                             && p.IsActive)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    return ApiResponseFactory.Failure<OrderDto>("Invalid product detected");

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
                             && d.TenantId == TenantId
                             && d.IsActive)
                    .ToListAsync();

                var taxesFromDb = await _db.Taxes
                    .Where(t => taxIds.Contains(t.TaxId)
                             && t.TenantId == TenantId
                             && t.IsActive)
                    .ToListAsync();

                var chargesFromDb = await _db.Charges
                    .Where(c => chargeIds.Contains(c.ChargeId)
                             && c.TenantId == TenantId
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

                // 🔹 Create Order entity
                var order = new Order
                {
                    TenantId = TenantId,
                    BranchId = dto.BranchId,
                    CustomerId = dto.CustomerId,
                    OrderDate = dto.OrderDate == default
                                ? DateTime.UtcNow
                                : dto.OrderDate,
                    Status = "Created",
                    SubTotal = subTotal,
                    DiscountTotal = discountTotal,
                    TaxTotal = taxTotal,
                    GrandTotal = grandTotal,
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
                return ApiResponseFactory.Failure<OrderDto>(
                    "Order creation failed",
                    new List<string> { ex.Message });
            }
        }


        public async Task<ApiResponse<List<OrderDto>>> GetOrdersAsync(
    PaginationRequest request,
    bool includeInactive)
        {
            // 1️⃣ Build query
            var query = _db.Orders
                .Where(o => o.TenantId == TenantId)
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
            var pagedDtoResult = new PagedResult<OrderDto>
            {
                Items = dtoList,
                Pagination = pagedResult.Pagination  // reuse repo pagination
            };

            // 5️⃣ Return ApiResponse
            return ApiResponseFactory.PagedSuccess(pagedDtoResult, "Orders fetched successfully");
        }

        // ✅ UPDATE STATUS ONLY
        public async Task<ApiResponse<string>> UpdateOrderAsync(int orderId, OrderUpdateDto dto)
        {
            var allowedStatuses = new[] { "Created", "Confirmed", "Completed", "Cancelled" };

            if (!allowedStatuses.Contains(dto.Status))
                return ApiResponseFactory.Failure<string>("Invalid status");

            var affectedRows = await _db.Orders
                .Where(o => o.OrderId == orderId && o.TenantId == TenantId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, dto.Status));

            if (affectedRows == 0)
                return ApiResponseFactory.Failure<string>("Order not found");

            return ApiResponseFactory.Success("Order updated successfully");
        }

        // ✅ DELETE ORDER
        public async Task<ApiResponse<string>> RemoveOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == TenantId);

            if (order == null)
                return ApiResponseFactory.Failure<string>("Order not found");

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Order deleted successfully");
        }
    }
}
