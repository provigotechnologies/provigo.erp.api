using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Models
{
    public class PdfOptions
    {
        public PaperSize PaperSize { get; set; } = PaperSize.A4;

        public bool ShowGST { get; set; }
        public bool ShowLogo { get; set; }
        public bool ShowQr { get; set; }

        public bool IsThermal =>
            PaperSize == PaperSize.Thermal58 ||
            PaperSize == PaperSize.Thermal80;
    }
}