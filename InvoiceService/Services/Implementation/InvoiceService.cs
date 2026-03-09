using InvoiceService.DTOs;
using InvoiceService.Infrastructure.Pdf;
using InvoiceService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using ProviGo.Common.Data;
using ProviGo.Common.Models;
using ProviGo.Common.Pagination;
using ProviGo.Common.Response;

namespace InvoiceService.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly TenantDbContext _db;

        public InvoiceService(TenantDbContext db)
        {
            _db = db;
        }


        private async Task<string> GenerateInvoiceNumber(Guid branchId, Guid tenantId)
        {
            var branch = await _db.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId &&
                                          b.TenantId == tenantId);

            if (branch == null)
                throw new Exception("Branch not found");

            var year = DateTime.UtcNow.Year;

            var lastInvoice = await _db.Invoices
                .Where(i => i.BranchId == branchId &&
                            i.TenantId == tenantId &&
                            i.InvoiceDate.Year == year)
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastInvoice != null)
            {
                var lastNumberPart = lastInvoice.InvoiceNumber.Split('-').Last();
                nextNumber = int.Parse(lastNumberPart) + 1;
            }

            return $"INV-{branch.BranchCode}-{year}-{nextNumber.ToString("D4")}";
        }



        public async Task<ApiResponse<InvoiceResponseDto>> GenerateFromOrderAsync(
     int orderId,
     Guid branchId,
     Guid tenantId)
        {
            try
            {
                // 1️⃣ Fetch Order with all related data
                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.OrderTaxes)
                        .ThenInclude(ot => ot.Tax)
                    .Include(o => o.OrderDiscounts)
                    .Include(o => o.Customer)
                    .Include(o => o.Branch)
                    .Include(o => o.TenantDetails)
                    .FirstOrDefaultAsync(o =>
                        o.OrderId == orderId &&
                        o.BranchId == branchId &&
                        o.TenantId == tenantId);

                if (order == null)
                    return ApiResponseFactory.Failure<InvoiceResponseDto>("Order not found");

                if (order.Status == "Cancelled")
                    return ApiResponseFactory.Failure<InvoiceResponseDto>("Cannot invoice cancelled order");

                // 2️⃣ Prevent duplicate invoice
                bool invoiceExists = await _db.Invoices
                    .AnyAsync(i =>
                        i.OrderId == orderId &&
                        i.BranchId == branchId &&
                        i.TenantId == tenantId);

                if (invoiceExists)
                    return ApiResponseFactory.Failure<InvoiceResponseDto>(
                        "Invoice already generated for this order");

                // 3️⃣ Calculate totals
                decimal totalTax = order.OrderTaxes?.Sum(t => t.TaxAmount) ?? 0;
                decimal totalDiscount = order.OrderDiscounts?.Sum(d => d.DiscountAmount) ?? 0;

                decimal subTotal = order.SubTotal;
                decimal grandTotal = subTotal - totalDiscount + totalTax;

                // 4️⃣ Generate Invoice Number
                string invoiceNumber = await GenerateInvoiceNumber(branchId, tenantId);

                // 5️⃣ Create Invoice
                var invoice = new Invoice
                {
                    TenantId = tenantId,
                    BranchId = branchId,
                    OrderId = order.OrderId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = DateTime.UtcNow,

                    // Company Snapshot
                    CompanyName = order.TenantDetails?.Name,
                    CompanyAddress = order.TenantDetails?.Address,
                    CompanyGSTIN = order.TenantDetails?.GSTIN,
                    CompanyPhone = order.TenantDetails?.Phone,

                    // Branch Snapshot
                    BranchName = order.Branch?.BranchName,
                    BranchAddress = order.Branch?.Address,
                    BranchGSTIN = order.Branch?.GSTIN,

                    // Customer Snapshot
                    CustomerName = order.Customer?.CustomerName,
                    CustomerPhone = order.Customer?.Phone,
                    CustomerAddress = order.Customer?.Address,
                    CustomerGSTIN = order.Customer?.GSTIN,

                    // Totals
                    SubTotal = subTotal,
                    DiscountTotal = totalDiscount,
                    TaxTotal = totalTax,
                    GrandTotal = grandTotal,

                    PaidAmount = order.PaidAmount,
                    BalanceAmount = grandTotal - order.PaidAmount,

                    IsGST = totalTax > 0,
                    Status = "Issued",
                    IsActive = true,

                    InvoiceItems = new List<InvoiceItem>()
                };

                // 6️⃣ Map OrderItems → InvoiceItems
                foreach (var item in order.OrderItems)
                {
                    invoice.InvoiceItems.Add(new InvoiceItem
                    {
                        ItemName = item.Product?.ProductName ?? "Item",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.TotalPrice
                    });
                }

                _db.Invoices.Add(invoice);

                // 7️⃣ Update Order Status
                order.Status = "Completed";

                await _db.SaveChangesAsync();

                // 8️⃣ Return Response
                var response = new InvoiceResponseDto
                {
                    InvoiceId = invoice.InvoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    SubTotal = invoice.SubTotal,
                    TaxAmount = invoice.TaxTotal,
                    DiscountAmount = invoice.DiscountTotal,
                    TotalAmount = invoice.GrandTotal
                };

                return ApiResponseFactory.Success(response, "Invoice generated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<InvoiceResponseDto>(
                    ex.Message,
                    new List<string> { ex.InnerException?.Message ?? "No inner exception" }
                );
            }
        }


        public async Task<ApiResponse<List<Invoice>>> GetInvoicesAsync(
            PaginationRequest request,
            bool includeInactive,
            Guid branchId,
            Guid tenantId)
        {
            try
            {
                var query = _db.Invoices
                    .Include(i => i.InvoiceItems)
                    .Where(i => i.TenantId == tenantId &&
                                i.BranchId == branchId);

                if (!includeInactive)
                    query = query.Where(i => i.IsActive);

                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return ApiResponseFactory.Success(data, "Invoices fetched");
            }
            catch (Exception ex)
            {
                return ApiResponseFactory.Failure<List<Invoice>>(
                    ex.Message,
                    ["Database error occurred"]);
            }
        }


        public async Task<byte[]> GenerateInvoicePdfAsync(
                    int invoiceId,
                    Guid branchId,
                    Guid tenantId,
                    PaperSize paperSize = PaperSize.A4) // ✅ dynamic size
        {
            var invoice = await _db.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i =>
                    i.InvoiceId == invoiceId &&
                    i.BranchId == branchId &&
                    i.TenantId == tenantId);

            if (invoice == null)
                throw new Exception("Invoice not found");

            var options = new PdfOptions
            {
                PaperSize = paperSize,
                ShowLogo = true,
                ShowGST = true,
                ShowQr = true
            };

            return InvoicePdfGenerator.Generate(invoice, options);
        }



        public async Task<ApiResponse<string>> CancelInvoiceAsync(
    int invoiceId,
    Guid branchId,
    Guid tenantId)
        {
            var invoice = await _db.Invoices
                .FirstOrDefaultAsync(i =>
                    i.InvoiceId == invoiceId &&
                    i.BranchId == branchId &&
                    i.TenantId == tenantId);

            if (invoice == null)
                return ApiResponseFactory.Failure<string>("Invoice not found");

            if (invoice.Status == "Paid")
                return ApiResponseFactory.Failure<string>("Paid invoice cannot be cancelled");

            invoice.Status = "Cancelled";
            invoice.IsActive = false;

            await _db.SaveChangesAsync();

            return ApiResponseFactory.Success("Invoice cancelled successfully");
        }
    }


}
