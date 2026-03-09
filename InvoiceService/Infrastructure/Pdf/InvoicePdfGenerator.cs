using ProviGo.Common.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace InvoiceService.Infrastructure.Pdf
{
    public static class InvoicePdfGenerator
    {
        public static byte[] Generate(Invoice invoice, PdfOptions options)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // ✅ Dynamic paper size
                    page.Size(PaperSizeResolver.Resolve(options.PaperSize));

                    // ✅ Margin based on paper
                    page.Margin(options.IsThermal ? 5 : 25);

                    page.DefaultTextStyle(x =>
                        x.FontSize(options.IsThermal ? 8 : 10));

                    page.Content().Column(col =>
                    {
                        if (options.IsThermal)
                            BuildThermalLayout(col, invoice, options);
                        else
                            BuildA4Layout(col, invoice, options);
                    });
                });
            }).GeneratePdf();
        }

        // ===============================
        // STANDARD LAYOUT: A4 / A5 / LETTER
        // ===============================
        private static void BuildA4Layout(ColumnDescriptor col, Invoice invoice, PdfOptions options)
        {
            // ===== Company Header =====
            col.Item().AlignCenter().Text(invoice.CompanyName).FontSize(18).Bold();
            col.Item().AlignCenter().Text(invoice.CompanyAddress);
            col.Item().AlignCenter().Text($"GSTIN: {invoice.CompanyGSTIN}");
            col.Item().AlignCenter().Text($"Phone: {invoice.CompanyPhone}");
            col.Item().LineHorizontal(1);

            // ===== Invoice + Branch Info =====
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text($"Invoice No: {invoice.InvoiceNumber}");
                    left.Item().Text($"Invoice Date: {invoice.InvoiceDate:dd-MM-yyyy}");
                    if (invoice.DueDate.HasValue)
                        left.Item().Text($"Due Date: {invoice.DueDate:dd-MM-yyyy}");
                    left.Item().Text($"Status: {invoice.Status}");
                });

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text($"Branch: {invoice.BranchName}");
                    right.Item().Text(invoice.BranchAddress);
                    right.Item().Text($"Branch GSTIN: {invoice.BranchGSTIN}");
                });
            });

            col.Item().PaddingVertical(5).LineHorizontal(1);

            // ===== Customer Info =====
            col.Item().Text("Bill To:").Bold();
            col.Item().Text(invoice.CustomerName);
            col.Item().Text(invoice.CustomerAddress);
            col.Item().Text($"Phone: {invoice.CustomerPhone}");
            col.Item().Text($"GSTIN: {invoice.CustomerGSTIN}");
            col.Item().PaddingVertical(10).LineHorizontal(1);

            // ===== Items Table =====
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);  // Item
                    columns.RelativeColumn(1);  // Qty
                    columns.RelativeColumn(2);  // Rate
                    columns.RelativeColumn(2);  // GST %
                    columns.RelativeColumn(2);  // GST Amt
                    columns.RelativeColumn(2);  // Line Total
                });

                table.Header(header =>
                {
                    header.Cell().Text("Item").Bold();
                    header.Cell().AlignCenter().Text("Qty").Bold();
                    header.Cell().AlignRight().Text("Rate").Bold();
                    header.Cell().AlignRight().Text("GST %").Bold();
                    header.Cell().AlignRight().Text("GST Amt").Bold();
                    header.Cell().AlignRight().Text("Total").Bold();
                });

                foreach (var item in invoice.InvoiceItems)
                {
                    table.Cell().Text(item.ItemName);
                    table.Cell().AlignCenter().Text(item.Quantity.ToString("0.##"));
                    table.Cell().AlignRight().Text(item.UnitPrice.ToString("0.00"));
                    table.Cell().AlignRight().Text(item.GSTPercent.ToString("0.00"));
                    table.Cell().AlignRight().Text(item.GSTAmount.ToString("0.00"));
                    table.Cell().AlignRight().Text(item.LineTotal.ToString("0.00"));
                }
            });

            col.Item().PaddingVertical(10).LineHorizontal(1);

            // ===== Totals =====
            col.Item().AlignRight().Column(total =>
            {
                total.Item().Text($"SubTotal: ₹{invoice.SubTotal:0.00}");
                total.Item().Text($"Discount: ₹{invoice.DiscountTotal:0.00}");
                if (options.ShowGST && invoice.IsGST)
                    total.Item().Text($"Tax Total: ₹{invoice.TaxTotal:0.00}");
                total.Item().Text($"Round Off: ₹{invoice.RoundOffAmount:0.00}");
                total.Item().Text($"Grand Total: ₹{invoice.GrandTotal:0.00}")
                    .Bold().FontSize(14);
                total.Item().PaddingTop(5).Text($"Paid: ₹{invoice.PaidAmount:0.00}");
                total.Item().Text($"Balance: ₹{invoice.BalanceAmount:0.00}");
            });

            col.Item().PaddingVertical(10).LineHorizontal(1);

            // ===== QR =====
            if (options.ShowQr)
            {
                col.Item().AlignCenter().Height(80).Image(GenerateQr(invoice.InvoiceNumber));
            }
        }

        // ===============================
        // THERMAL LAYOUT: 58 / 80 mm
        // ===============================
        private static void BuildThermalLayout(ColumnDescriptor col, Invoice invoice, PdfOptions options)
        {
            // Company
            col.Item().AlignCenter().Text(invoice.CompanyName).FontSize(10).Bold();
            col.Item().AlignCenter().Text(invoice.CompanyAddress);
            col.Item().AlignCenter().Text($"Ph: {invoice.CompanyPhone}");
            col.Item().LineHorizontal(1);

            // Invoice Info
            col.Item().Text($"Inv: {invoice.InvoiceNumber}");
            col.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");
            col.Item().Text($"Customer: {invoice.CustomerName}");
            col.Item().LineHorizontal(1);

            // Items
            foreach (var item in invoice.InvoiceItems)
            {
                col.Item().Text(item.ItemName).Bold();
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"{item.Quantity:0.##} x {item.UnitPrice:0.00}");
                    row.ConstantItem(60).AlignRight().Text($"{item.LineTotal:0.00}");
                });
            }

            col.Item().LineHorizontal(1);

            // Totals
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("SubTotal");
                row.ConstantItem(60).AlignRight().Text($"{invoice.SubTotal:0.00}");
            });

            if (options.ShowGST && invoice.IsGST)
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Tax");
                    row.ConstantItem(60).AlignRight().Text($"{invoice.TaxTotal:0.00}");
                });
            }

            col.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL").Bold();
                row.ConstantItem(60).AlignRight().Text($"{invoice.GrandTotal:0.00}").Bold();
            });

            col.Item().Text($"Paid: {invoice.PaidAmount:0.00}");
            col.Item().Text($"Balance: {invoice.BalanceAmount:0.00}");

            col.Item().LineHorizontal(1);

            // QR
            if (options.ShowQr)
            {
                col.Item().AlignCenter().Height(60).Image(GenerateQr(invoice.InvoiceNumber));
            }

            col.Item().AlignCenter().Text("Thank You!").Italic();
        }

        // ===============================
        // QR CODE GENERATOR
        // ===============================
        private static byte[] GenerateQr(string data)
        {
            var qr = new QRCodeGenerator();
            var qrData = qr.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            return new PngByteQRCode(qrData).GetGraphic(10);
        }
    }
}