using ProviGo.Common.Models;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace InvoiceService.Infrastructure.Pdf
{
    public static class PaperSizeResolver
    {
        private const float MmToPoints = 2.83465f;

        public static PageSize Resolve(PaperSize size)
        {
            return size switch
            {
                PaperSize.A4 => PageSizes.A4,
                PaperSize.A5 => PageSizes.A5,
                PaperSize.Letter => PageSizes.Letter,

                PaperSize.Thermal58 => new PageSize(
                    58 * MmToPoints,
                    1000), // Large height instead of infinity

                PaperSize.Thermal80 => new PageSize(
                    80 * MmToPoints,
                    1000),

                _ => PageSizes.A4
            };
        }
    }
}